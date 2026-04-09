using DataIngestion.Application.Abstractions;
using DataIngestion.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DataIngestion.Api.Controllers
{
    [ApiController]
    [Route("api/statistics")]
    public class StatsController : Controller
    {
        private readonly IStatsQueryService _statsQueryService;

        public StatsController(IStatsQueryService statsQueryService)
        {
            _statsQueryService = statsQueryService;
        }


        [HttpGet("summary")]
        [ProducesResponseType(typeof(GetSummaryStatsResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<GetSummaryStatsResponse>> GetSummary(CancellationToken cancellationToken)
        {
            return await _statsQueryService.GetSummaryAsync(cancellationToken);
        }
    }
}
