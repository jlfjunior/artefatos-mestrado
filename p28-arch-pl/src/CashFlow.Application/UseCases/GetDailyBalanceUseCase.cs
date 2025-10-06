using CashFlow.Application.Commands;
using CashFlow.Application.Queries;
using CashFlow.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using static CashFlow.Application.ErrorCode;

namespace CashFlow.Application.UseCases;

public class
    GetDailyBalanceUseCase : IRequestHandler<GetDailyBalanceQuery, CommandResponse<GetDailyBalanceQueryResponse>>
{
    private readonly ILogger<GetDailyBalanceUseCase> _logger;
    private readonly ICashFlowRepository _repository;

    public GetDailyBalanceUseCase(ICashFlowRepository repository, ILogger<GetDailyBalanceUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CommandResponse<GetDailyBalanceQueryResponse>> Handle(GetDailyBalanceQuery query,
        CancellationToken cancellationToken)
    {
        var cashFlow = await _repository.GetCurrentCashByAccountId(query.AccountId);

        if (cashFlow == null)
        {
            var msgError = $"Cash flow Id:{query.AccountId} not found.";
            _logger.LogError(msgError);
            return CommandResponse<GetDailyBalanceQueryResponse>.CreateFail(CashFlowNotFound, msgError);
        }

        var response = new GetDailyBalanceQueryResponse
        {
            CurrentBalance = cashFlow.GetBalance(),
            Transactions = cashFlow.Transactions!,
            Date = cashFlow.Date,
            AccountId = cashFlow.AccountId
        };
        _logger.LogInformation($"Cash flow Id:{query.AccountId}, was found.");
        return CommandResponse<GetDailyBalanceQueryResponse>.CreateSuccess(response);
    }
}