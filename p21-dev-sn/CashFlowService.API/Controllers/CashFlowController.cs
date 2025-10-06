using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace CashFlowService.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CashFlowController : ControllerBase
    {
        private readonly ICashFlowService _cashFlowService;

        public CashFlowController(ICashFlowService cashFlowService)
        {
            _cashFlowService = cashFlowService;
        }

        [HttpPost("transactions")]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            if (request == null)
                return BadRequest("Request payload is invalid.");

            var result = await _cashFlowService.CreateTransactionAsync(request);
            return Ok(result);
        }

        [HttpGet("transactions/{date}")]
        public async Task<IActionResult> GetTransactionsByDate(string date)
        {
            var transactions = await _cashFlowService.GetTransactionsByDateAsync(date);
            return Ok(transactions);
        }
    }
}