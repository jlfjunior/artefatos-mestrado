using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Boxflux.Application.Commands.Lauchings.LauchingCommand;

namespace Boxflux.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LauchingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LauchingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateLauchingCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lauchings = await _mediator.Send(new GetAllLauchingQuery());
            return Ok(lauchings);
        }
    }
}
