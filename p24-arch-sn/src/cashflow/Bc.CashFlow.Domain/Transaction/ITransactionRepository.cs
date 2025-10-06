using Bc.CashFlow.Domain.DbContext;

namespace Bc.CashFlow.Domain.Transaction;

public interface ITransactionRepository
{
	Task<IEnumerable<Identity<int>>> GetTransactionsId(
		int? userId,
		int? accountId,
		TransactionType? transactionType,
		decimal? amountFrom,
		decimal? amountTo,
		DateTime? transactionDateSince,
		DateTime? transactionDateUntil,
		DateTime? projectedRepaymentDateSince,
		DateTime? projectedRepaymentDateUntil,
		int? pagingSkip,
		int? pagingLimit,
		CancellationToken cancellationToken);

	Task<IEnumerable<Identity<int>>> GetTransactionsIdByProjectedRepaymentDate(
		int? accountId,
		DateTime projectedRepaymentDate,
		CancellationToken cancellationToken);

	Task<ITransaction?> GetSingleTransaction(
		int? accountId,
		CancellationToken cancellationToken);

	Task<Identity<int>?> CreateTransaction(
		int? userId,
		int accountId,
		TransactionType transactionType,
		decimal amount,
		string? description,
		DateTime transactionDate,
		decimal? transactionFee,
		DateTime? projectedRepaymentDate,
		CancellationToken cancellationToken);

	Task<ITransaction?> GetTransaction(
		int transactionId,
		CancellationToken cancellationToken);
}
