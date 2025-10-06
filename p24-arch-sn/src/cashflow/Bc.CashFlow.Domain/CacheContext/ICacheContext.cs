using System.Diagnostics.CodeAnalysis;
using Bc.CashFlow.Domain.Account;
using Bc.CashFlow.Domain.AccountType;
using Bc.CashFlow.Domain.Transaction;
using Bc.CashFlow.Domain.User;

namespace Bc.CashFlow.Domain.CacheContext;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public interface ICacheContext
{
	ICacheCollection<IUser> User { get; }
	ICacheCollection<IAccountType> AccountType { get; }
	ICacheCollection<IAccount> Account { get; }
	ICacheCollection<ITransaction> Transaction { get; }
}
