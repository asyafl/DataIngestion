using DataIngestion.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace DataIngestion.Application.Abstractions
{
    public interface IBatchIngestionService
    {
        Task<IngestBatchResponse> IngestBatchAsync(IFormFile file, CancellationToken cancellationToken = default);
    }
}
