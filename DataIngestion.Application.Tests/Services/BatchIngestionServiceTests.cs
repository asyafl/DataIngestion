using DataIngestion.Application.Abstractions;
using DataIngestion.Application.DTOs;
using DataIngestion.Application.Exceptions;
using DataIngestion.Application.Services;
using DataIngestion.Application.Tests.Helpers;
using DataIngestion.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Serilog;

namespace DataIngestion.Application.Tests.Services
{
    public class BatchIngestionServiceTests
    {
        private readonly Mock<ITransactionRepository> _repositoryMock = new();
        private readonly Mock<IDeduplicationKeyGenerator> _deduplicationKeyMock = new();
        private readonly Mock<IValidator<IngestBatchRowDto>> _validatorMock = new();
        private readonly Mock<ILogger> _loggerMock = new();

        [Fact]
        public async Task IngestBatchAsync_ShouldThrowDomainValidationException_WhenFileIsNull()
        {
            var service = CreateService();

            var act = async () => await service.IngestBatchAsync(null!);

            await act.Should().ThrowAsync<DomainValidationException>()
                .WithMessage("File is required for ingestion.");
        }

        [Fact]
        public async Task IngestBatchAsync_ShouldThrowDomainValidationException_WhenFileIsNotCsv()
        {
            var service = CreateService();
            var file = FormFileFactory.CreateCsv("batch.txt", "ignored");

            var act = async () => await service.IngestBatchAsync(file);

            await act.Should().ThrowAsync<DomainValidationException>()
                .WithMessage("Only CSV files are accepted.");
        }

        [Fact]
        public async Task IngestBatchAsync_ShouldAcceptSingleValidRow()
        {
            var csv = "CustomerId,TransactionDateUtc,Amount,Currency,SourceChannel\ncust-1,2025-01-10T10:15:00Z,100.25,USD,WEB";
            var file = FormFileFactory.CreateCsv("batch.csv", csv);
            var service = CreateService();

            _validatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<IngestBatchRowDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _deduplicationKeyMock
                .Setup(x => x.Generate("cust-1", It.IsAny<DateTime>(), 100.25m, "USD", "WEB"))
                .Returns("dedup-1");
            _repositoryMock
                .Setup(x => x.ExistsByDeduplicationKeyAsync("dedup-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _repositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _repositoryMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await service.IngestBatchAsync(file);

            result.TotalRows.Should().Be(1);
            result.AcceptedRows.Should().Be(1);
            result.RejectedRows.Should().Be(0);
            result.Errors.Should().BeEmpty();

            _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task IngestBatchAsync_ShouldRejectRow_WhenValidationFails()
        {
            var csv = "CustomerId,TransactionDateUtc,Amount,Currency,SourceChannel\ncust-1,2025-01-10T10:15:00Z,100.25,USD,WEB";
            var file = FormFileFactory.CreateCsv("batch.csv", csv);
            var service = CreateService();
            var validationFailure = new ValidationFailure("Amount", "Amount must be greater than 0.");

            _validatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<IngestBatchRowDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult([validationFailure]));

            var result = await service.IngestBatchAsync(file);

            result.TotalRows.Should().Be(1);
            result.AcceptedRows.Should().Be(0);
            result.RejectedRows.Should().Be(1);
            result.Errors.Should().ContainSingle();
            result.Errors[0].Error.Should().Contain("Row 2:");
            result.Errors[0].Error.Should().Contain("Amount must be greater than 0.");

            _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
            _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task IngestBatchAsync_ShouldRejectRow_WhenDuplicateDetected()
        {
            var csv = "CustomerId,TransactionDateUtc,Amount,Currency,SourceChannel\ncust-1,2025-01-10T10:15:00Z,100.25,USD,WEB";
            var file = FormFileFactory.CreateCsv("batch.csv", csv);
            var service = CreateService();

            _validatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<IngestBatchRowDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _deduplicationKeyMock
                .Setup(x => x.Generate("cust-1", It.IsAny<DateTime>(), 100.25m, "USD", "WEB"))
                .Returns("dedup-1");
            _repositoryMock
                .Setup(x => x.ExistsByDeduplicationKeyAsync("dedup-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await service.IngestBatchAsync(file);

            result.TotalRows.Should().Be(1);
            result.AcceptedRows.Should().Be(0);
            result.RejectedRows.Should().Be(1);
            result.Errors.Should().ContainSingle();
            result.Errors[0].Error.Should().Be("Row 2: Duplicate transaction detected.");

            _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
            _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task IngestBatchAsync_ShouldRejectRowWithUnexpectedError_WhenRepositoryThrows()
        {
            var csv = "CustomerId,TransactionDateUtc,Amount,Currency,SourceChannel\ncust-1,2025-01-10T10:15:00Z,100.25,USD,WEB";
            var file = FormFileFactory.CreateCsv("batch.csv", csv);
            var service = CreateService();

            _validatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<IngestBatchRowDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _deduplicationKeyMock
                .Setup(x => x.Generate("cust-1", It.IsAny<DateTime>(), 100.25m, "USD", "WEB"))
                .Returns("dedup-1");
            _repositoryMock
                .Setup(x => x.ExistsByDeduplicationKeyAsync("dedup-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _repositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("db failure"));

            var result = await service.IngestBatchAsync(file);

            result.TotalRows.Should().Be(1);
            result.AcceptedRows.Should().Be(0);
            result.RejectedRows.Should().Be(1);
            result.Errors.Should().ContainSingle();
            result.Errors[0].Error.Should().Be("Row 2: Unexpected error occurred.");

            _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        private BatchIngestionService CreateService()
        {
            return new BatchIngestionService(
                _repositoryMock.Object,
                _deduplicationKeyMock.Object,
                _validatorMock.Object,
                _loggerMock.Object);
        }
    }
}
