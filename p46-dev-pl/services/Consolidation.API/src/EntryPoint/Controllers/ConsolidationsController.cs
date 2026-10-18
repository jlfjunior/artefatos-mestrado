using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Shared.DTOs;
using Consolidation.API.Application.Services;
using Consolidation.API.src.Application.Interface;

namespace Consolidation.API.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsolidationsController : ControllerBase
{
    private readonly IConsolidationService _consolidationService;
    private readonly ILogger<ConsolidationsController> _logger;

    public ConsolidationsController(IConsolidationService consolidationService, ILogger<ConsolidationsController> logger)
    {
        _consolidationService = consolidationService ?? throw new ArgumentNullException(nameof(consolidationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{date}")]
    [ProducesResponseType(typeof(ConsolidationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConsolidationByDate(
        [FromRoute] DateTime date,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _consolidationService.GetByDateAsync(date, cancellationToken);
            if (result == null)
                return NotFound();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter consolidação");
            return StatusCode(500, new ErrorResponse
            {
                Code = "INTERNAL_ERROR",
                Message = "Erro ao obter consolidação"
            });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginationResponse<ConsolidationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListConsolidations(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var result = await _consolidationService.GetByDateRangeAsync(
                startDate, endDate, pageNumber, pageSize, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar consolidações");
            return StatusCode(500, new ErrorResponse
            {
                Code = "INTERNAL_ERROR",
                Message = "Erro ao listar consolidações"
            });
        }
    }
}
