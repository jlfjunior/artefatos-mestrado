using Bc.CashFlow.Domain.DbContext;

namespace Bc.CashFlow.Domain.Transaction;

public interface ITransactionService
{
	Task<IEnumerable<ITransaction>> GetTransactions(
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

	Task<IEnumerable<ITransaction>> GetTransactionsOnProjectedRepaymentDate(
		int? accountId,
		DateTime projectedRepaymentDate,
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

	Task PublishNewTransactionToBalance(
		int id,
		CancellationToken cancellationToken);

	Task<ITransaction?> GetTransaction(
		int transactionId,
		CancellationToken cancellationToken);
}
