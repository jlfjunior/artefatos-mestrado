using EasyCash.Abstractions.Exceptions;

namespace EasyCash.Report.Domain.Consolidations.Errors;

public static class ConsolidationsErrors
{
    public static readonly Error AlreadyConsolidatedDay = new(
        "Consolidation.AlreadyConsolidatedDay",
        "The day has already been consolidated");

    public static readonly Error ConsolidationDateCannotBeInFuture = new(
        "Consolidation.ConsolidationCannotBeInFuture",
        "Consolidation date cannot be in the future");
}

