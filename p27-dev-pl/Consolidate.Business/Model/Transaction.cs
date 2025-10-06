namespace Consolidate.Business.Model
{
    public record Transaction(Guid TransactionId, string Description, double Value, DateTime Date, TransactionType Type) { }
}