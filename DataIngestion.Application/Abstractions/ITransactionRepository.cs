using DataIngestion.Domain.Entities;

namespace DataIngestion.Application.Abstractions
{
    public interface ITransactionRepository
    {
        Task<bool> ExistsByDeduplicationKeyAsync(string deduplicationKey, CancellationToken cancellationToken = default);
        Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
