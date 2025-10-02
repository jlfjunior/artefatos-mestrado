using CashFlowControl.Core.Application.DTOs;
using CashFlowControl.Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CashFlowControl.DailyConsolidation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsolidatedBalanceController : ControllerBase
    {
        private readonly IDailyConsolidationService _dailyConsolidationService;

        public ConsolidatedBalanceController(IDailyConsolidationService dailyConsolidationService)
        {
            _dailyConsolidationService = dailyConsolidationService;
        }

        [Authorize]
        [HttpGet("{date}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerResponse(200, "Busca realizada com sucesso.", typeof(ConsolidatedBalanceDayDTO))]
        public async Task<ActionResult<ConsolidatedBalanceDayDTO>> GetConsolidatedBalance(DateTime date)
        {
            var resultBalance = await _dailyConsolidationService.GetConsolidatedBalanceByDateAsync(date.Date);
            if (!resultBalance.IsSuccess)
            {
                if (resultBalance.HasValidationErrors)
                {
                    return BadRequest(new { Errors = resultBalance.ValidationErrors });
                }

                return StatusCode(500, resultBalance.SystemError);
            }

            var balance = resultBalance.Value;
            if (balance == null)
                return NotFound("No balance found for the given date.");

            return Ok(balance);
        }
    }
}
