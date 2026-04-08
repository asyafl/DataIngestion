using DataIngestion.Application.Abstractions;
using DataIngestion.Application.Exceptions;
using DataIngestion.Application.Services;
using DataIngestion.Application.Tests.Helpers;
using DataIngestion.Domain.Entities;
using FluentAssertions;
using Moq;
using Serilog;

namespace DataIngestion.Application.Tests.Services
{
    public class TransactionIngestionServiceTests
    {
        private readonly Mock<IDeduplicationKeyGenerator> _deduplicationKeyMock = new();
        private readonly Mock<ITransactionRepository> _repositoryMock = new();
        private readonly Mock<ILogger> _loggerMock = new();

        [Fact]
        public async Task IngestAsync_ShouldThrowDuplicateTransactionException_WhenDuplicateExists()
        {
            var request = TestDataFactory.CreateValidTransactionRequest();
            var service = CreateService();

            _deduplicationKeyMock
                .Setup(x => x.Generate(request.CustomerId, request.TransactionDateUtc, request.Amount, request.Currency, request.SourceChannel))
                .Returns("dup-key");
            _repositoryMock
                .Setup(x => x.ExistsByDeduplicationKeyAsync("dup-key", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var act = async () => await service.IngestAsync(request);

            await act.Should().ThrowAsync<DuplicateTransactionException>()
                .WithMessage("Transaction already exists.");

            _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
            _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task IngestAsync_ShouldPersistAndReturnResponse_WhenTransactionIsNew()
        {
            var request = TestDataFactory.CreateValidTransactionRequest();
            var service = CreateService();
            Transaction? capturedTransaction = null;

            _deduplicationKeyMock
                .Setup(x => x.Generate(request.CustomerId, request.TransactionDateUtc, request.Amount, request.Currency, request.SourceChannel))
                .Returns("new-key");
            _repositoryMock
                .Setup(x => x.ExistsByDeduplicationKeyAsync("new-key", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _repositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                .Callback<Transaction, CancellationToken>((t, _) =>
                {
                    capturedTransaction = t;
                    t.Id = 123;
                })
                .Returns(Task.CompletedTask);
            _repositoryMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var before = DateTime.UtcNow;
            var result = await service.IngestAsync(request);
            var after = DateTime.UtcNow;

            result.Id.Should().Be(123);
            result.CustomerId.Should().Be(request.CustomerId);
            result.TransactionDateUtc.Should().Be(request.TransactionDateUtc);
            result.Amount.Should().Be(request.Amount);
            result.Currency.Should().Be(request.Currency);
            result.SourceChannel.Should().Be(request.SourceChannel);
            result.CreatedAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);

            capturedTransaction.Should().NotBeNull();
            capturedTransaction!.DeduplicationKey.Should().Be("new-key");
            _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        private TransactionIngestionService CreateService()
        {
            return new TransactionIngestionService(
                _deduplicationKeyMock.Object,
                _repositoryMock.Object,
                _loggerMock.Object);
        }
    }
}
