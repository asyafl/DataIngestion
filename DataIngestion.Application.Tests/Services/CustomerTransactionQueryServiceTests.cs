using DataIngestion.Application.Abstractions;
using DataIngestion.Application.DTOs;
using DataIngestion.Application.Services;
using DataIngestion.Application.Tests.Helpers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Serilog;

namespace DataIngestion.Application.Tests.Services
{
    public class CustomerTransactionQueryServiceTests
    {
        private readonly Mock<ITransactionRepository> _repositoryMock = new();
        private readonly Mock<IValidator<GetCustomerTransactionsRequest>> _validatorMock = new();
        private readonly Mock<ILogger> _loggerMock = new();

        [Fact]
        public async Task GetPagedAsync_ShouldThrowArgumentException_WhenCustomerIdIsEmpty()
        {
            var service = CreateService();
            var request = TestDataFactory.CreateValidCustomerTransactionsRequest();

            var act = async () => await service.GetPagedAsync(string.Empty, request);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Customer ID cannot be null or empty.*");
        }

        [Fact]
        public async Task GetPagedAsync_ShouldThrowValidationException_WhenQueryIsInvalid()
        {
            var service = CreateService();
            var request = TestDataFactory.CreateValidCustomerTransactionsRequest();
            var failures = new List<ValidationFailure> { new("Page", "Page must be greater than 0.") };

            _validatorMock
                .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            var act = async () => await service.GetPagedAsync("customer-1", request);

            await act.Should().ThrowAsync<ValidationException>();
            _repositoryMock.Verify(x => x.GetByCustomerAsync(It.IsAny<string>(), It.IsAny<GetCustomerTransactionsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetPagedAsync_ShouldReturnRepositoryResult_WhenRequestIsValid()
        {
            var service = CreateService();
            var request = TestDataFactory.CreateValidCustomerTransactionsRequest();
            var expected = new PagedResult<CustomerTransactionItemResponse>
            {
                Page = 1,
                PageSize = 20,
                TotalCount = 1,
                TotalPages = 1,
                Items = new List<CustomerTransactionItemResponse>
                {
                    new()
                    {
                        Id = 1,
                        CustomerId = "customer-1",
                        TransactionDateUtc = DateTime.UtcNow.AddDays(-1),
                        Amount = 50m,
                        Currency = "USD",
                        SourceChannel = "WEB",
                        CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
                    }
                }
            };

            _validatorMock
                .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _repositoryMock
                .Setup(x => x.GetByCustomerAsync("customer-1", request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await service.GetPagedAsync("customer-1", request);

            result.Should().BeSameAs(expected);
            _repositoryMock.Verify(x => x.GetByCustomerAsync("customer-1", request, It.IsAny<CancellationToken>()), Times.Once);
        }

        private CustomerTransactionQueryService CreateService()
        {
            return new CustomerTransactionQueryService(
                _repositoryMock.Object,
                _validatorMock.Object,
                _loggerMock.Object);
        }
    }
}
