using System.Diagnostics.CodeAnalysis;

namespace Bc.CashFlow.Domain.Account;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public interface IAccount
{
	int Id { get; }
	int UserId { get; }
	int AccountTypeId { get; }
	string Name { get; }
	decimal InitialBalance { get; }
	decimal CurrentBalance { get; }
	DateTime BalanceUpdatedAt { get; }
	DateTime CreatedAt { get; }
}