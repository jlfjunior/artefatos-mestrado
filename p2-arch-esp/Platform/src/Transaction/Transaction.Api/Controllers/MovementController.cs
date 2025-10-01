using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transaction.Application.UseCases.Transaction.Create;

namespace Transaction.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MovementController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MovementController> _logger;

        public MovementController(IMediator mediator, ILogger<MovementController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Create([FromBody] CreateMovementInput input, CancellationToken cancellation)
        {
            _logger.LogInformation("Creating movement: {Description}", input.Description);

            try
            {
                var output = await _mediator.Send(input, cancellation);

                _logger.LogInformation("Movement created successfully: {Description}", output.Description);
                
                return CreatedAtAction(nameof(Create), output);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Failed to create movement: {Reason}", ex.Message);
                return BadRequest(new ProblemDetails
                {
                    Title = "Failed to create movement",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
        }
    }
}
