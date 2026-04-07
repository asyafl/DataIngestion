using DataIngestion.Application.Abstractions;
using DataIngestion.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DataIngestion.Api.Controllers
{

    [ApiController]
    [Route("api/ingest")]
    public class IngestionController : Controller
    {
        private readonly ITransactionIngestionService _service;

        public IngestionController(ITransactionIngestionService service)
        {
            _service = service;
        }

        [HttpPost("transaction")]
        [ProducesResponseType(typeof(IngestTransactionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<IngestTransactionResponse>> IngestTransaction([FromBody] IngestTransactionRequest request, CancellationToken cancellationToken)
        {
            return await _service.IngestAsync(request, cancellationToken);
        }
    }
}
