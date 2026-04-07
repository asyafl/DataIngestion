using DataIngestion.Application.Abstractions;
using DataIngestion.Application.DTOs;
using DataIngestion.Application.Exceptions;
using DataIngestion.Domain.Entities;
using Serilog;

namespace DataIngestion.Application.Services
{
    public class TransactionIngestionService : ITransactionIngestionService
    {
        private readonly IDeduplicationKeyGenerator _deduplicationKeyGenerator;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILogger _logger;

        public TransactionIngestionService(IDeduplicationKeyGenerator deduplicationKeyGenerator, ITransactionRepository transactionRepository, ILogger logger)
        {
            _deduplicationKeyGenerator = deduplicationKeyGenerator;
            _transactionRepository = transactionRepository;
            _logger = logger;
        }


        public async Task<IngestTransactionResponse> IngestAsync(IngestTransactionRequest dto, CancellationToken cancellationToken = default)
        {
            var dKey = _deduplicationKeyGenerator.Generate(dto.CustomerId, dto.TransactionDateUtc, dto.Amount, dto.Currency, dto.SourceChannel);

            if (await _transactionRepository.ExistsByDeduplicationKeyAsync(dKey, cancellationToken))
            {
                _logger.Warning("Duplicate transaction detected for CustomerId: {CustomerId}, " +
                    "TransactionDateUtc: {TransactionDateUtc}, Amount: {Amount}, Currency: {Currency}," +
                    " SourceChannel: {SourceChannel}. Skipping ingestion.",
                    dto.CustomerId, dto.TransactionDateUtc, dto.Amount, dto.Currency, dto.SourceChannel);
                throw new DuplicateTransactionException("Transaction already exists.");
            }

            var transaction = new Transaction
            {
                CustomerId = dto.CustomerId,
                TransactionDateUtc = dto.TransactionDateUtc,
                Amount = dto.Amount,
                Currency = dto.Currency,
                SourceChannel = dto.SourceChannel,
                DeduplicationKey = dKey,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _transactionRepository.AddAsync(transaction, cancellationToken);
            await _transactionRepository.SaveChangesAsync(cancellationToken);

            _logger.Information("Transaction ingested successfully for CustomerId: {CustomerId}, " +
                "TransactionDateUtc: {TransactionDateUtc}, Amount: {Amount}, Currency: {Currency}," +
                " SourceChannel: {SourceChannel}.",
                dto.CustomerId, dto.TransactionDateUtc, dto.Amount, dto.Currency, dto.SourceChannel);

            return new IngestTransactionResponse
            {
                Id = transaction.Id,
                CustomerId = dto.CustomerId,
                TransactionDateUtc = dto.TransactionDateUtc,
                Amount = dto.Amount,
                Currency = dto.Currency,
                SourceChannel = dto.SourceChannel,
                CreatedAtUtc = transaction.CreatedAtUtc
            };
        }
    }
}
