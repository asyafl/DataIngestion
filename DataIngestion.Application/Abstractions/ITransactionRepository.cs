using DataIngestion.Application.DTOs;
using DataIngestion.Domain.Entities;

namespace DataIngestion.Application.Abstractions
{
    public interface ITransactionRepository
    {
        Task<bool> ExistsByDeduplicationKeyAsync(string deduplicationKey, CancellationToken cancellationToken = default);
        Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        Task<PagedResult<CustomerTransactionItemResponse>> GetByCustomerAsync(string customerId, GetCustomerTransactionsRequest query,
            CancellationToken cancellationToken = default);

        Task<GetSummaryStatsResponse> GetSummaryStatsAsync(CancellationToken cancellationToken = default);
    }
}
