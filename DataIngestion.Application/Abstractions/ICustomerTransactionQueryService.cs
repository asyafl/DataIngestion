using DataIngestion.Application.DTOs;

namespace DataIngestion.Application.Abstractions
{
    public interface ICustomerTransactionQueryService
    {
        Task<PagedResult<CustomerTransactionItemResponse>> GetPagedAsync(string customerId, GetCustomerTransactionsRequest query, CancellationToken cancellationToken = default);
    }
}
