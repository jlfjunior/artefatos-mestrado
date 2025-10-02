using Application.Launch.Launch.Command.CreateLaunch;
using Application.Launch.Launch.Query.GetAllLaunches;
using Domain.Models.Launch.Launch;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LaunchController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LaunchController> _logger;

    public LaunchController(IMediator mediator, ILogger<LaunchController> logger)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LaunchModel launch)
    {
        try
        {
            _logger.LogDebug($"Launch attempt");

            if (launch == null)
            {
                _logger.LogError("Invalid launch request.");
                return BadRequest("Invalid launch request.");
            }

            var launchCommand = new CreateLaunchCommand()
            {
                LaunchType = launch.LaunchType,
                ProductsOrder = launch.ProductsOrder
            };

            var response = await _mediator.Send(launchCommand);
            _logger.LogDebug($"Launch in {EnumHelper.GetEnumDescription(launchCommand.LaunchType)} created successfully.");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to launch {launch.LaunchType}: {ex.Message}");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _mediator.Send(new GetAllLaunchesQuery());

            _logger.LogInformation("Retrieved all launches.");
            return Ok(result);
        }
        catch (Exception)
        {
            _logger.LogError("Failed to retrieve launches.");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}
