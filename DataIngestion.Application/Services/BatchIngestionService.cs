using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using DataIngestion.Application.Abstractions;
using DataIngestion.Application.DTOs;
using DataIngestion.Application.Exceptions;
using DataIngestion.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Globalization;

namespace DataIngestion.Application.Services
{
    public class BatchIngestionService : IBatchIngestionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IDeduplicationKeyGenerator _deduplicationKeyGenerator;
        private readonly IValidator<IngestBatchRowDto> _validator;
        private readonly ILogger _logger;

        public BatchIngestionService(
            ITransactionRepository transactionRepository,
            IDeduplicationKeyGenerator deduplicationKey,
            IValidator<IngestBatchRowDto> validator,
            ILogger logger)
        {
            _transactionRepository = transactionRepository;
            _deduplicationKeyGenerator = deduplicationKey;
            _validator = validator;
            _logger = logger;
        }
        public async Task<IngestBatchResponse> IngestBatchAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file is null || file.Length == 0)
            {
                _logger.Error("No file provided for ingestion.");
                throw new DomainValidationException("File is required for ingestion.");
            }

            if (Path.GetExtension(file.FileName)?.ToLower() != ".csv")
            {
                _logger.Error("Invalid file format: {FileName}. Only CSV files are accepted.", file.FileName);
                throw new DomainValidationException("Only CSV files are accepted.");
            }

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null,
                HeaderValidated = null,
                BadDataFound = null
            };

            await using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, csvConfig);

            await csv.ReadAsync();
            csv.ReadHeader();

            var rowNumber = 1; // header
            var result = new IngestBatchResponse
            {
                FileName = file.FileName,
                Errors = new List<BatchErrorResponse>()
            };

            while (await csv.ReadAsync())
            {
                rowNumber++;
                result.TotalRows++;

                try
                {
                    var dto = new IngestBatchRowDto
                    {
                        RowNumber = rowNumber,
                        CustomerId = csv.GetField<string>("CustomerId") ?? string.Empty,
                        TransactionDateUtc = csv.GetField<DateTime>("TransactionDateUtc"),
                        Amount = csv.GetField<decimal>("Amount"),
                        Currency = csv.GetField<string>("Currency") ?? string.Empty,
                        SourceChannel = csv.GetField<string>("SourceChannel") ?? string.Empty
                    };

                    var validationResult = await _validator.ValidateAsync(dto, cancellationToken);

                    if (!validationResult.IsValid)
                    {
                        result.RejectedRows++;
                        var errorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                        result.Errors.Add(new BatchErrorResponse
                        {
                            CustomerId = dto.CustomerId,
                            RowNumber = rowNumber,
                            Error = $"Row {rowNumber}: {errorMessage}"
                        });
                        continue;
                    }

                    var deduplicationKey = _deduplicationKeyGenerator.Generate(dto.CustomerId, dto.TransactionDateUtc, dto.Amount, dto.Currency, dto.SourceChannel);

                    if (await _transactionRepository.ExistsByDeduplicationKeyAsync(deduplicationKey, cancellationToken))
                    {
                        result.RejectedRows++;
                        result.Errors.Add(new BatchErrorResponse
                        {
                            CustomerId = dto.CustomerId,
                            RowNumber = rowNumber,
                            Error = $"Row {rowNumber}: Duplicate transaction detected."
                        });
                        continue;
                    }

                    var transaction = new Transaction
                    {
                        CustomerId = dto.CustomerId,
                        TransactionDateUtc = EnsureUtc(dto.TransactionDateUtc),
                        Amount = dto.Amount,
                        Currency = dto.Currency,
                        SourceChannel = dto.SourceChannel,
                        DeduplicationKey = deduplicationKey
                    };
                    await _transactionRepository.AddAsync(transaction, cancellationToken);
                    await _transactionRepository.SaveChangesAsync(cancellationToken);

                    result.AcceptedRows++;
                }
                catch (Exception ex) when (
                ex is FormatException ||
                ex is TypeConverterException ||
                ex is ReaderException ||
                ex is HeaderValidationException ||
                ex is DomainValidationException)
                {
                    _logger.Error(ex, "Validation error processing row {RowNumber} in file {FileName}.", rowNumber, file.FileName);
                    result.RejectedRows++;
                    result.Errors.Add(new BatchErrorResponse
                    {
                        RowNumber = rowNumber,
                        Error = ex.Message
                    });
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error processing row {RowNumber} in file {FileName}.", rowNumber, file.FileName);
                    result.RejectedRows++;
                    result.Errors.Add(new BatchErrorResponse
                    {
                        CustomerId = string.Empty,
                        RowNumber = rowNumber,
                        Error = $"Row {rowNumber}: Unexpected error occurred."
                    });
                }
            }
            _logger.Information(
            "Batch import completed. Total: {TotalRows}, Accepted: {AcceptedRows}, Rejected: {RejectedRows}",
            result.TotalRows,
            result.AcceptedRows,
            result.RejectedRows);

            return result;
        }


        private static DateTime EnsureUtc(DateTime dateTime)
        {
            return dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
                _ => dateTime
            };
        }
    }
}

