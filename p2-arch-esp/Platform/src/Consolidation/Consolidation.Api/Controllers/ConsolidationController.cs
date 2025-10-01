using Consolidation.Application.UseCases.Consolidation.Commons;
using Consolidation.Application.UseCases.Consolidation.Get;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Consolidation.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConsolidationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ConsolidationController> _logger;

        public ConsolidationController(IMediator mediator, ILogger<ConsolidationController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet(Name = "consolidations")]
        [ProducesResponseType(typeof(ConsolidateListOutput), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> List([FromBody] GetConsolidateInput input, CancellationToken cancellation)
        {
            _logger.LogInformation("Listing consolidations");

            try
            {
                var output = await _mediator.Send(input, cancellation);
                return Ok(output);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Failed to listing consolidations: {Reason}", ex.Message);
                return BadRequest(new ProblemDetails
                {
                    Title = "Failed to listing consolidation",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
        }
    }
}
