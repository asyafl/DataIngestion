using DataIngestion.Application.DTOs;

namespace DataIngestion.Application.Abstractions
{
    public interface ITransactionIngestionService
    {
        Task<IngestTransactionResponse> IngestAsync(IngestTransactionRequest dto, CancellationToken cancellationToken = default);
    }
}
