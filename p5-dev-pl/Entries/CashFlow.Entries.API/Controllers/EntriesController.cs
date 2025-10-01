using CashFlow.Entries.Application.DTOs;
using CashFlow.Entries.Domain.Exceptions;
using CashFlow.Entries.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Entries.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntriesController : ControllerBase
    {
        private readonly ILogger<EntriesController> _logger;
        private readonly IEntryService _entrieService;

        public EntriesController(ILogger<EntriesController> logger, IEntryService entrieService)
        {
            _logger = logger;
            _entrieService = entrieService;

        }

        [HttpPost]
        [ProducesResponseType(201)]
        public async Task<IActionResult> CreateEntrie([FromBody] CreateEntrieInput input)
        {
            try
            {
                var entrie = await _entrieService.CreateAsync(input.Value, input.Description, input.Type);

                if (entrie == null)
                {
                    _logger.LogError("Failed to create entry.");
                    return BadRequest("Failed to create entry.");
                }

                return StatusCode(201);
            }
            catch (EntityValidationFailException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the entry.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }
    }
}
