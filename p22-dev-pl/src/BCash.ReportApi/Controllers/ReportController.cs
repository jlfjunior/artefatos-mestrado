using AutoMapper;
using BCash.Domain.DTOs;
using BCash.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BCash.ReportApi.Controllers
{
    [ApiController]
    [Route("v1/report")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        private readonly IBalanceService _balanceService;

        private readonly IMapper _mapper;

        public ReportController(ITransactionService transactionService, IBalanceService balanceService, IMapper mapper)
        {
            _transactionService = transactionService;
            _balanceService = balanceService;
            _mapper = mapper;
        }

        [HttpGet("balance")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBalanceWithOffsetPagination([FromQuery] DateTime? initDate, [FromQuery] DateTime? endDate, int pageNumber = 1, int pageSize = 10)
        {
            if (!initDate.HasValue || !endDate.HasValue)
                return BadRequest("Init date and end date are required.");

            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest($"{nameof(pageNumber)} and {nameof(pageSize)} size must be greater than 0.");

            var pagedBalances =
                await _balanceService.GetBalancePagedAsync(initDate.Value, endDate.Value, pageNumber, pageSize);

            var pagedBalancesDTO =
                _mapper.Map<PagedResponseOffsetDto<BalanceDto>>(pagedBalances);

            return Ok(pagedBalancesDTO);
        }

        [HttpGet("transactions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTransactionsWithOffsetPagination([FromQuery] DateTime? initDate, [FromQuery] DateTime? endDate, int pageNumber = 1, int pageSize = 10)
        {
            if (!initDate.HasValue || !endDate.HasValue)
                return BadRequest("Init date and end date are required.");

            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest($"{nameof(pageNumber)} and {nameof(pageSize)} size must be greater than 0.");

            var pagedTransactions =
                await _transactionService.GetTransactionPagedAsync(initDate.Value, endDate.Value, pageNumber, pageSize);

            var pagedTransactionsDTO =
                _mapper.Map<PagedResponseOffsetDto<TransactionDto>>(pagedTransactions);

            return Ok(pagedTransactionsDTO);
        }
    }
}
