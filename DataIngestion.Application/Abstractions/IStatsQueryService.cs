using DataIngestion.Application.DTOs;

namespace DataIngestion.Application.Abstractions
{
    public interface IStatsQueryService
    {
        Task<GetSummaryStatsResponse> GetSummaryAsync(CancellationToken cancellationToken = default);
    }
}
