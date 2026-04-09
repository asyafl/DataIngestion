using DataIngestion.Application.Abstractions;
using DataIngestion.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DataIngestion.Api.Controllers
{

    [ApiController]
    [Route("api/ingest")]
    public class IngestionController : Controller
    {
        private readonly ITransactionIngestionService _transactionService;
        private readonly IBatchIngestionService _batchService;

        public IngestionController(ITransactionIngestionService transactionService, IBatchIngestionService batchService)
        {
            _transactionService = transactionService;
            _batchService = batchService;
        }

        [HttpPost("transaction")]
        [ProducesResponseType(typeof(IngestTransactionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<IngestTransactionResponse>> IngestTransaction([FromBody] IngestTransactionRequest request, CancellationToken cancellationToken)
        {
            var response = await _transactionService.IngestAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPost("batch")]
        [ProducesResponseType(typeof(IngestBatchResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IngestBatchResponse>> IngestBatchTransactions(IFormFile file, CancellationToken cancellationToken)
        {
            var response = await _batchService.IngestBatchAsync(file, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, response);
        }

    }
}
