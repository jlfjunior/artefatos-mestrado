namespace Bc.CashFlow.Domain.Transaction;

public class NegativePaymentDueDaysException : Exception
{
	public NegativePaymentDueDaysException() : base("Negative payment due days are not allowed.")
	{
	}
}
