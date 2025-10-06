using System.Diagnostics.CodeAnalysis;

namespace Bc.CashFlow.Domain.AccountType;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public interface IAccountTypeBusiness
{
	Task<IEnumerable<IAccountType>> GetAccountTypes(
		string? name,
		decimal? baseFeeFrom,
		decimal? baseFeeTo,
		int? paymentDueDaysFrom,
		int? paymentDueDaysTo,
		int? pagingSkip,
		int? pagingLimit,
		CancellationToken cancellationToken);

	Task<IAccountType?> GetAccountType(
		int id,
		CancellationToken cancellationToken);
}
