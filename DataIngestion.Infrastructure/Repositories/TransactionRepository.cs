using DataIngestion.Application.Abstractions;
using DataIngestion.Application.DTOs;
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

        public async Task<PagedResult<CustomerTransactionItemResponse>> GetByCustomerAsync(string customerId, GetCustomerTransactionsRequest query, CancellationToken cancellationToken = default)
        {
            var transactionQuery = _appDbContext.Transactions
                .AsNoTracking()
                .Where(x => x.CustomerId == customerId);

            if (query.FromDate.HasValue)
            {
                var fromDateUtc = EnsureUtc(query.FromDate.Value);
                transactionQuery = transactionQuery.Where(x => x.TransactionDateUtc >= fromDateUtc);
            }

            if (query.ToDate.HasValue)
            {
                var toDateUtc = EnsureUtc(query.ToDate.Value);
                transactionQuery = transactionQuery.Where(x => x.TransactionDateUtc <= toDateUtc);
            }

            if (!string.IsNullOrWhiteSpace(query.Currency))
            {
                var currency = query.Currency.Trim();
                transactionQuery = transactionQuery.Where(x => x.Currency == currency);
            }

            if (!string.IsNullOrWhiteSpace(query.SourceChannel))
            {
                var sourceChannel = query.SourceChannel.Trim();
                transactionQuery = transactionQuery.Where(x => x.SourceChannel == sourceChannel);
            }

            var totalCount = await transactionQuery.CountAsync(cancellationToken);

            var items = await transactionQuery
           .OrderByDescending(x => x.TransactionDateUtc)
           .ThenByDescending(x => x.Id)
           .Skip((query.Page - 1) * query.PageSize)
           .Take(query.PageSize)
           .Select(x => new CustomerTransactionItemResponse
           {
               Id = x.Id,
               CustomerId = x.CustomerId,
               TransactionDateUtc = x.TransactionDateUtc,
               Amount = x.Amount,
               Currency = x.Currency,
               SourceChannel = x.SourceChannel,
               CreatedAtUtc = x.CreatedAtUtc
           })
           .ToListAsync(cancellationToken);

            return new PagedResult<CustomerTransactionItemResponse>
            {
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize),
                Items = items
            };
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        private static DateTime EnsureUtc(DateTime dateTime)
        {
            return dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
                _ => dateTime
            };
        }

        public async Task<GetSummaryStatsResponse> GetSummaryStatsAsync(CancellationToken cancellationToken = default)
        {
            var transactionsQuery = _appDbContext.Transactions.AsNoTracking();

            var totalTransactions = await transactionsQuery.CountAsync(cancellationToken);
            var totalAmount = await transactionsQuery.SumAsync(x => x.Amount, cancellationToken);
            var uniqueCustomers = await transactionsQuery.Select(x => x.CustomerId).Distinct().CountAsync(cancellationToken);

            var byCurrency = await transactionsQuery
                .GroupBy(x => x.Currency)
                .Select(g => new StatsByCurrencyResponse
                {
                    Currency = g.Key,
                    TotalAmount = g.Sum(x => x.Amount),
                    Count = g.Count()
                })
                .ToListAsync(cancellationToken);

            var bySourceChannel = await transactionsQuery
                .GroupBy(x => x.SourceChannel)
                .Select(g => new StatsBySourceChannelResponse
                {
                    SourceChannel = g.Key,
                    Count = g.Count()
                })
                .ToListAsync(cancellationToken);

            return new GetSummaryStatsResponse
            {
                TotalTransactions = totalTransactions,
                TotalAmount = totalAmount,
                UniqueCustomers = uniqueCustomers,
                ByCurrency = byCurrency,
                BySourceChannel = bySourceChannel
            };
        }
    }
}
