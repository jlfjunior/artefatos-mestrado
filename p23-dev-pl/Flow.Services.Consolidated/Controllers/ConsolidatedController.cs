using Flow.Application.UseCases.Consolidated;
using Flow.Core.Entities;
using Flow.Infra.Data;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Flow.Services.Consolidated.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsolidatedController : ControllerBase
    {

        [HttpGet("{date}")]
        public async Task<IActionResult> Get(DateTime date, [FromServices] IGetDailyBalanceUseCase useCase)
        {
            var dailyBalance = await useCase.Execute(date);
            if (dailyBalance is null)
            {
                return NotFound("Balance not found for this date");
            }
            return Ok(dailyBalance);
        }
    }
}
