using System.Diagnostics.CodeAnalysis;
using Bc.CashFlow.Domain.DbContext;
using Bc.CashFlow.Domain.Transaction;

namespace Bc.CashFlow.Domain.Account;

[SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
public interface IAccountService
{
	Task<IEnumerable<IAccount>> GetAccounts(
		int? pagingSkip,
		int? pagingLimit,
		CancellationToken cancellationToken);

	Task<IEnumerable<IAccount>> GetAccounts(
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

	Task<IEnumerable<Identity<int>>> GetAccountsId(
		CancellationToken cancellationToken);

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

	Task<IAccount?> GetAccount(
		int id,
		CancellationToken cancellationToken);

	Task UpdateBalance(
		int accountId,
		decimal adjustedAmount,
		CancellationToken cancellationToken);
}
