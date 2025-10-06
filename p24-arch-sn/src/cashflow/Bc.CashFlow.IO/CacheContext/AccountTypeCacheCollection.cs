using System.Diagnostics.CodeAnalysis;
using Bc.CashFlow.Domain.AccountType;
using StackExchange.Redis;

namespace Bc.CashFlow.IO.CacheContext;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class AccountTypeCacheCollection : BaseCacheCollection<IAccountType, AccountTypeCacheCollection.AccountTypeDto>
{
	public AccountTypeCacheCollection(
		IDatabase db)
		: base("user", db)
	{
	}

	public record AccountTypeDto : IAccountType
	{
		public required int Id { get; init; }
		public required string Name { get; init; }
		public required decimal BaseFee { get; init; }
		public required int PaymentDueDays { get; init; }
	}
}