using DataIngestion.Application.Abstractions;
using DataIngestion.Application.DTOs;
using DataIngestion.Application.Services;
using FluentAssertions;
using Moq;

namespace DataIngestion.Application.Tests.Services
{
    public class StatsQueryServiceTests
    {
        [Fact]
        public async Task GetSummaryAsync_ShouldReturnRepositoryResult()
        {
            var repositoryMock = new Mock<ITransactionRepository>();
            var service = new StatsQueryService(repositoryMock.Object);
            var expected = new GetSummaryStatsResponse
            {
                TotalTransactions = 10,
                TotalAmount = 500m,
                UniqueCustomers = 3
            };

            repositoryMock
                .Setup(x => x.GetSummaryStatsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await service.GetSummaryAsync();

            result.Should().BeSameAs(expected);
            repositoryMock.Verify(x => x.GetSummaryStatsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
