using Flow.Application.UseCases.Transaction;
using Flow.Core.DTOs;
using Flow.Services.Transactions.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Flow.Services.Transactions.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(AuthenticatedUserFilter))]
    public class TransactionController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TransactionDTO request, [FromServices] IExecuteTransactionUseCase useCase)
        {
            await useCase.Execute(request);
            return Created();
        }
    }
}
