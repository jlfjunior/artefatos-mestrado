using Domain.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CashEntryController : ControllerBase
    {
        private readonly ICashEntryService _cashEntryService;
        public CashEntryController(ICashEntryService cashEntryService)
        {
            _cashEntryService = cashEntryService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCashEntry([FromBody] CreateCashEntryRequest request, CancellationToken cancellationToken)
        {
            var result = await _cashEntryService.CreateCashEntryAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("/DailyEntries")]
        public async Task<IActionResult> GetDailyCashEntries([FromQuery] GetDailyCashEntriesResquest request, CancellationToken cancellationToken)
        {
            var result = await _cashEntryService.GetDailyCashEntriesAsync(request, cancellationToken);
            return Ok(result);
        }

    }
}
