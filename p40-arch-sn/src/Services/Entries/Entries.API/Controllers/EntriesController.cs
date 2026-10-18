using Entries.Application.DTOs;
using Entries.Application.UseCases.CreateEntry;
using Entries.Application.UseCases.GetEntriesByDate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Entries.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class EntriesController(
        CreateEntryHandler createEntryHandler,
        GetEntriesByDateHandler getEntriesByDateHandler) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(EntryResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync(
            [FromBody] CreateEntryRequest request,
            CancellationToken cancellationToken)
        {
            var response = await createEntryHandler.HandleAsync(request, cancellationToken);
            return Created(string.Empty, response);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<EntryResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByDateAsync(
            [FromQuery] DateTime date,
            CancellationToken cancellationToken)
        {
            var response = await getEntriesByDateHandler.HandleAsync(date, cancellationToken);
            return Ok(response);
        }
    }
}
