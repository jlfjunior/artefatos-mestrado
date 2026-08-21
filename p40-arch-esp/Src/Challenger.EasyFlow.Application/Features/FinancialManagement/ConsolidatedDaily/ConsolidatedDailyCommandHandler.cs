using Challenger.EasyFlow.Application.Common.CQRS;
using Challenger.EasyFlow.Domain.Aggregates.DailyBalanceAggregate;
using Microsoft.Extensions.Logging;

namespace Challenger.EasyFlow.Application.Features.FinancialManagement.ConsolidatedDaily;

public sealed class ConsolidatedDailyCommandHandler(IDailyBalanceRepository dailyBalanceRepository,
                                                    ILogger<ConsolidatedDailyCommandHandler> logger)
    : ICommandHandler<ConsolidatedDailyCommand>
{
    private readonly IDailyBalanceRepository _dailyBalanceRepository = dailyBalanceRepository;
    private readonly ILogger<ConsolidatedDailyCommandHandler> _logger = logger;

    public async ValueTask HandleAsync(ConsolidatedDailyCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting consolidation of daily balances for {Date} at: {DateTimeUTC}", command.Period.ToShortDateString(), DateTimeOffset.UtcNow);
        var balances = await _dailyBalanceRepository
            .ListAllBydDateAsync(command.Period, cancellationToken);

        // TODO: Aqui podemos consolidar os dados, calcular totais, gerar relatórios, etc. e enviar via email, atualizar um dashboard, etc. 

        _logger.LogInformation("Consolidated daily balances for {Date}: {@Balances}", command.Period.ToShortDateString(), balances);
    }
}