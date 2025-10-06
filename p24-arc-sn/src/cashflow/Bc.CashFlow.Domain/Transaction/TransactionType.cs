using System.Diagnostics.CodeAnalysis;

namespace Bc.CashFlow.Domain.Transaction;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum TransactionType
{
	Debit = 0,
	Credit = 1
}