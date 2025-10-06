namespace Bc.CashFlow.Domain.AccountType;

public interface IAccountType
{
	int Id { get; }
	string Name { get; }
	decimal BaseFee { get; }
	int PaymentDueDays { get; }
}