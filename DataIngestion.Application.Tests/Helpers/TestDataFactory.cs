using DataIngestion.Application.DTOs;

namespace DataIngestion.Application.Tests.Helpers
{
    internal static class TestDataFactory
    {
        public static IngestTransactionRequest CreateValidTransactionRequest()
        {
            return new IngestTransactionRequest
            {
                CustomerId = "customer-1",
                TransactionDateUtc = DateTime.UtcNow.AddMinutes(-5),
                Amount = 120.50m,
                Currency = "USD",
                SourceChannel = "WEB"
            };
        }

        public static GetCustomerTransactionsRequest CreateValidCustomerTransactionsRequest()
        {
            return new GetCustomerTransactionsRequest
            {
                Page = 1,
                PageSize = 20,
                Currency = "USD",
                SourceChannel = "WEB"
            };
        }
    }
}
