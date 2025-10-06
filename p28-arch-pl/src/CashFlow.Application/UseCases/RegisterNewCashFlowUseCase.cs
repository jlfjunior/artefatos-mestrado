using CashFlow.Application.Commands;
using CashFlow.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using static CashFlow.Application.ErrorCode;

namespace CashFlow.Application.UseCases;

public class RegisterNewCashFlowUseCase : IRequestHandler<RegisterNewCashFlowCommand, CommandResponse<Guid>>
{
    private readonly ILogger<RegisterNewCashFlowUseCase> _logger;
    private readonly ICashFlowService _service;

    public RegisterNewCashFlowUseCase(ICashFlowService service, ILogger<RegisterNewCashFlowUseCase> logger)
    {
        _logger = logger;
        _service = service;
    }

    public async Task<CommandResponse<Guid>> Handle(RegisterNewCashFlowCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _service.RegisterNewAggregate(command.AccountId);
            _logger.LogInformation("CashFlow Id: {0}, registered!", aggregate!.Id);
            return CommandResponse<Guid>.CreateSuccess(aggregate.Id);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Erro interno");
            return CommandResponse<Guid>.CreateFail(InternalError, "Internal Error");
        }
    }
}