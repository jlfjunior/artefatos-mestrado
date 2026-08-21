using Consolidation.Application.UseCases.GetDailyReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consolidation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class ConsolidationController(
        GetDailyReportHandler getDailyReportHandler) : ControllerBase
    {
        [HttpGet("{date}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByDateAsync(
            DateTime date,
            CancellationToken cancellationToken)
        {
            var response = await getDailyReportHandler.HandleAsync(date, cancellationToken);

            if (response is null)
                return NotFound($"Nenhuma consolidação encontrada para a data {date:yyyy-MM-dd}.");

            return Ok(response);
        }

        [HttpGet("range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByRangeAsync(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            CancellationToken cancellationToken)
        {
            var response = await getDailyReportHandler.HandleRangeAsync(from, to, cancellationToken);
            return Ok(response);
        }
    }
}
