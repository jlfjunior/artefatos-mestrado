using CashFlow.Application.Commands;
using CashFlow.Domain.Exceptions;
using CashFlow.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using static CashFlow.Application.ErrorCode;


namespace CashFlow.Application.UseCases;

public class CancelTransactionUseCase : IRequestHandler<CancelTransactionCommand, CommandResponse<Guid>>
{
    private readonly ICashFlowService _cashFlowService;
    private readonly ILogger<CancelTransactionUseCase> _logger;

    public CancelTransactionUseCase(ICashFlowService cashFlowService, ILogger<CancelTransactionUseCase> logger)
    {
        _cashFlowService = cashFlowService;
        _logger = logger;
    }

    public async Task<CommandResponse<Guid>> Handle(CancelTransactionCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var transaction = await _cashFlowService.ReverseTransaction(command.TransactionId);

            if (transaction == null)
            {
                var msgError = $"Transaction Id:{command.TransactionId} cannot be canceled or reversed";
                _logger.LogError(msgError);
                return CommandResponse<Guid>.CreateFail(TransactionNotFound, msgError);
            }

            _logger.LogInformation("Transaction Id:{0} was successfully canceled or reversed.", transaction.Id);
            return CommandResponse<Guid>.CreateSuccess(transaction.Id);
        }
        catch (TransactionNotFoundException ex)
        {
            _logger.LogError(ex, ex.Message);
            return CommandResponse<Guid>.CreateFail(TransactionNotFound, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error canceling transaction.");
            return CommandResponse<Guid>.CreateFail(InternalError, "Internal Error");
        }
    }
}