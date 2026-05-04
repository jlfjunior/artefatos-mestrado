using FluxoCaixa.Consolidado.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FluxoCaixa.Consolidado.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConsolidadoController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConsolidadoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{data}")]
    public async Task<IActionResult> GetByData(string data)
    {
        try
        {
            var result = await _mediator.Send(new GetConsolidadoQuery(data));
            Response.Headers.Append("X-Cache-TTL", "60");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("range")]
    public async Task<IActionResult> GetRange([FromQuery] string inicio, [FromQuery] string fim)
    {
        try
        {
            var result = await _mediator.Send(new GetConsolidadoRangeQuery(inicio, fim));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}
