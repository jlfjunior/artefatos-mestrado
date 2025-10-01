using CashFlow.DailyConsolidated.Application.DTOs;
using CashFlow.DailyConsolidated.Application.Extensions;
using CashFlow.DailyConsolidated.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.DailyConsolidated.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DailyConsolidatedController : ControllerBase
    {
        private readonly ILogger<DailyConsolidatedController> _logger;
        private readonly IDailyConsolidatedService _dailyConsolidatedService;

        public DailyConsolidatedController(ILogger<DailyConsolidatedController> logger, IDailyConsolidatedService dailyConsolidatedService)
        {
            _logger = logger;
            _dailyConsolidatedService = dailyConsolidatedService;
        }

        [HttpGet("{date}")]
        public async Task<ActionResult<DailyConsolidationOutput>> GetDailyConsolidated(DateOnly date)
        {
            var consolidated = await _dailyConsolidatedService.GetAsync(date);
            if (consolidated == null)
                return NotFound();

            return Ok(consolidated.ToOutput());
        }
    }
}
