namespace CashFlow.Domain.Exceptions;

public class CashFlowNotFoundException : Exception
{
    public CashFlowNotFoundException(string message) : base(message)
    {
    }
}