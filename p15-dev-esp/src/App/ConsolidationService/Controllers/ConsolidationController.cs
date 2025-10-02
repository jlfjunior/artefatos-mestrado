using Application.Consolidation.Consolidation.Query.GetAllConsolidations;
using Application.Product.Product.Query.GetPaginatedConsolidations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsolidationService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ConsolidationController(IMediator _mediator, ILogger<ConsolidationController> _logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _mediator.Send(new GetAllConsolidationsQuery());

            _logger.LogInformation("Retrieved all consolidations.");
            return Ok(result);
        }
        catch (Exception)
        {
            _logger.LogError("Failed to retrieve consolidations.");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("/api/Consolidation/Paginated")]
    public async Task<IActionResult> GetPaginated(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var result = await _mediator.Send(new GetPaginatedConsolidationsQuery { PageNumber = pageNumber, PageSize = pageSize });

            _logger.LogInformation("Retrieved paginated consolidations.");

            return Ok(new
            {
                Total = result.TotalCount,
                Consolidations = result.Items,
                result.PageSize,
                result.PageNumber
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve consolidations.");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}
