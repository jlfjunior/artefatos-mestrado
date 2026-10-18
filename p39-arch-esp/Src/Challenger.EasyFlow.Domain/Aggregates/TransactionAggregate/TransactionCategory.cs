namespace Challenger.EasyFlow.Domain.Aggregates.TransactionAggregate;

public sealed record TransactionCategory(int Id, string Name)
{
    public readonly static TransactionCategory Sale = new(1, "Sale");
    public readonly static TransactionCategory Expense = new(2, "Expense");

    public static IEnumerable<TransactionCategory> AsList() => [Sale, Expense];

    public static TransactionCategory FromId(int to)
        => AsList().Single(x => x.Id == to);
}
