using Bc.CashFlow.Domain.Account;
using Bc.CashFlow.Domain.DbContext;
using Bc.CashFlow.Domain.User;

namespace Bc.CashFlow.Domain.Transaction;

public interface ITransactionBusiness
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

	Task<IEnumerable<IAccount>> GetAccounts(
		int? pagingSkip,
		int? pagingLimit,
		CancellationToken cancellationToken);

	Task<Identity<int>> CreateTransaction(
		int? userId,
		int accountId,
		TransactionType transactionType,
		decimal amount,
		string? description,
		DateTime transactionDate,
		CancellationToken cancellationToken);

	Task UpdateAccountBalance(
		int transactionId,
		CancellationToken cancellationToken);

	Task<ITransaction> GetRequiredTransaction(
		int id,
		CancellationToken cancellationToken);

	Task<IUser> GetRequiredUser(
		int id,
		CancellationToken cancellationToken);
}
