using MediatR;
using Microsoft.AspNetCore.Mvc;
using static ConsolidatedDailyCommand;

namespace Boxflux.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsolidatedDailyController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ConsolidatedDailyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{date}")]
        public async Task<IActionResult> GetConsolidatedDaily(DateTime date)
        {
            var balance = await _mediator.Send(new GetConsolidatedDailyQuery { Date = date });
            return Ok(balance);
        }
    }
}
