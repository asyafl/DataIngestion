using DataIngestion.Application.Abstractions;
using DataIngestion.Domain.Entities;
using DataIngestion.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DataIngestion.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _appDbContext;
        public TransactionRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
        {
            await _appDbContext.Transactions.AddAsync(transaction, cancellationToken);
        }

        public async Task<bool> ExistsByDeduplicationKeyAsync(string deduplicationKey, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Transactions.AnyAsync(x => x.DeduplicationKey == deduplicationKey, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
