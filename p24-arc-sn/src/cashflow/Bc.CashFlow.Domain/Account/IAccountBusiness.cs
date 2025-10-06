using System.Diagnostics.CodeAnalysis;

namespace Bc.CashFlow.Domain.Account;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public interface IAccountBusiness
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

	Task<IAccount?> GetAccount(
		int id,
		CancellationToken cancellationToken);

	Task<IAccount> GetRequiredAccount(
		int id,
		CancellationToken cancellationToken);
}
