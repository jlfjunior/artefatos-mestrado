using System.Diagnostics.CodeAnalysis;
using Bc.CashFlow.Domain.Transaction;
using StackExchange.Redis;
using ITransaction = Bc.CashFlow.Domain.Transaction.ITransaction;

namespace Bc.CashFlow.IO.CacheContext;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class TransactionCacheCollection : BaseCacheCollection<ITransaction, TransactionCacheCollection.TransactionDto>
{
	public TransactionCacheCollection(
		IDatabase db)
		: base("transaction", db)
	{
	}

	public record TransactionDto : ITransaction
	{
		public required int Id { get; init; }
		public required int? UserId { get; init; }
		public required int AccountId { get; init; }
		public required TransactionType TransactionType { get; init; }
		public required decimal Amount { get; init; }
		public required string? Description { get; init; }
		public required DateTime TransactionDate { get; init; }
		public required decimal? TransactionFee { get; init; }
		public required DateTime? ProjectedRepaymentDate { get; init; }
	}
}
