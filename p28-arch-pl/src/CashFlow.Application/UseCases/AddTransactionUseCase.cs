using CashFlow.Application.Commands;
using CashFlow.Domain.Exceptions;
using CashFlow.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using static CashFlow.Application.ErrorCode;

namespace CashFlow.Application.UseCases;

public class AddTransactionUseCase : IRequestHandler<AddTransactionDailyCommand, CommandResponse<Guid>>
{
    private readonly ICashFlowService _domainService;
    private readonly ILogger<AddTransactionUseCase> _logger;

    public AddTransactionUseCase(ICashFlowService domainService, ILogger<AddTransactionUseCase> logger)
    {
        _domainService = domainService;
        _logger = logger;
    }

    public async Task<CommandResponse<Guid>> Handle(AddTransactionDailyCommand dailyCommand,
        CancellationToken cancellationToken)
    {
        try
        {
            var transaction =
                await _domainService.AddTransaction(dailyCommand.AccountId, dailyCommand.Amount, dailyCommand.Type);

            _logger.LogInformation($"Transaction Id:{transaction!.Id}, registered");
            return CommandResponse<Guid>.CreateSuccess(transaction.Id);
        }
        catch (CashFlowNotFoundException ex)
        {
            _logger.LogError(ex, ex.Message);
            return CommandResponse<Guid>.CreateFail(CashFlowNotFound, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Erro interno");
            return CommandResponse<Guid>.CreateFail(InternalError, "Internal Error");
        }
    }
}