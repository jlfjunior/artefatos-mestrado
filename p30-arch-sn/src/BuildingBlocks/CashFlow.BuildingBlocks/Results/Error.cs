namespace CashFlow.BuildingBlocks.Results;

public sealed record Error(
    string Code,
    string Message,
    ErrorType Type,
    Dictionary<string, string[]>? Details = null)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Unexpected);
}