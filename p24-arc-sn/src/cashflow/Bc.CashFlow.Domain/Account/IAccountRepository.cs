using Bc.CashFlow.Domain.DbContext;

namespace Bc.CashFlow.Domain.Account;

public interface IAccountRepository
{
	Task<IEnumerable<Identity<int>>> GetAccountsId(
		int? userId,
		int? accountTypeId,
		string? name,
		decimal? initialBalanceFrom,
		decimal? initialBalanceTo,
		decimal? currentBalanceFrom,
		decimal? currentBalanceTo,
		DateTime? balanceUpdatedAtSince,
		DateTime? balanceUpdatedAtUntil,
		DateTime? createdAtSince,
		DateTime? createdAtUntil,
		int? pagingSkip,
		int? pagingLimit,
		CancellationToken cancellationToken);

	Task<int> GetAccountsTotal(
		int? userId,
		int? accountTypeId,
		string? name,
		decimal? initialBalanceFrom,
		decimal? initialBalanceTo,
		decimal? currentBalanceFrom,
		decimal? currentBalanceTo,
		DateTime? balanceUpdatedAtSince,
		DateTime? balanceUpdatedAtUntil,
		DateTime? createdAtSince,
		DateTime? createdAtUntil,
		int? pagingSkip,
		int? pagingLimit,
		CancellationToken cancellationToken);

	Task<IAccount?> GetAccount(
		int id,
		CancellationToken cancellationToken);

	Task UpdateBalance(
		int accountId,
		decimal adjustedAmount,
		CancellationToken cancellationToken);
}
