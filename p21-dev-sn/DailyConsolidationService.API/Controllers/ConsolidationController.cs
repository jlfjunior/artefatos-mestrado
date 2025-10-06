using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace DailyConsolidationService.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ConsolidationController : ControllerBase
    {
        private readonly IConsolidationService _consolidationService;

        public ConsolidationController(IConsolidationService consolidationService)
        {
            _consolidationService = consolidationService;
        }

        [HttpGet("report/{date}")]
        public async Task<IActionResult> GetDailyReport(string date)
        {
            var report = await _consolidationService.GenerateDailyReportAsync(date);
            return Ok(report);
        }
    }
}