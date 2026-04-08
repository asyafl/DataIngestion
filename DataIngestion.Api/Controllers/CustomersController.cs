using DataIngestion.Application.Abstractions;
using DataIngestion.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DataIngestion.Api.Controllers
{
    [ApiController]
    [Route("customers")]
    public class CustomersController : Controller
    {
        private readonly ICustomerTransactionQueryService _customerTransactionQueryService;

        public CustomersController(ICustomerTransactionQueryService customerTransactionQueryService)
        {
            _customerTransactionQueryService = customerTransactionQueryService;
        }


        [HttpGet("{customerId}/transactions")]
        [ProducesResponseType(typeof(PagedResult<CustomerTransactionItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedResult<CustomerTransactionItemResponse>>> GetPagedTransactions(
            [FromRoute] string customerId, [FromQuery] GetCustomerTransactionsRequest request, CancellationToken cancellationToken)
        {

            return await _customerTransactionQueryService.GetPagedAsync(customerId, request, cancellationToken);

        }
    }
}
