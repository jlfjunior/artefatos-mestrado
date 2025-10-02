using EasyCash.Abstractions.Messaging.Commands;

namespace EasyCash.Report.Command.Consolidations.DailyConsolidations;

public sealed class DailyConsolidationCommand : ICommand<DailyConsolidationCommandResult>
{
    public DateOnly Date { get; set; }
}

public sealed class DailyConsolidationCommandResult : ICommandResult
{
    public Guid Id { get; set; }

    public static DailyConsolidationCommandResult Ok(Guid id) =>
        new()
        {
            Id = id
        };
}

