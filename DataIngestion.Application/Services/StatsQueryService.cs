using DataIngestion.Application.Abstractions;
using DataIngestion.Application.DTOs;

namespace DataIngestion.Application.Services
{
    public class StatsQueryService : IStatsQueryService
    {
        private readonly ITransactionRepository _transactionRepository;

        public StatsQueryService(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<GetSummaryStatsResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            return await _transactionRepository.GetSummaryStatsAsync(cancellationToken);
        }
    }
}
