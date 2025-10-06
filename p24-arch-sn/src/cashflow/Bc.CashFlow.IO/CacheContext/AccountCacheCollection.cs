using System.Diagnostics.CodeAnalysis;
using Bc.CashFlow.Domain.Account;
using StackExchange.Redis;

namespace Bc.CashFlow.IO.CacheContext;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class AccountCacheCollection : BaseCacheCollection<IAccount, AccountCacheCollection.AccountDto>
{
	public AccountCacheCollection(
		IDatabase db)
		: base("account", db)
	{
	}

	public record AccountDto : IAccount
	{
		public required int Id { get; init; }
		public required int UserId { get; init; }
		public required int AccountTypeId { get; init; }
		public required string Name { get; init; }
		public required decimal InitialBalance { get; init; }
		public required decimal CurrentBalance { get; init; }
		public required DateTime BalanceUpdatedAt { get; init; }
		public required DateTime CreatedAt { get; init; }
	}
}