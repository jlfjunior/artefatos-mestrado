using Bc.CashFlow.Domain.DbContext;

namespace Bc.CashFlow.Domain.AccountType;

public interface IAccountTypeRepository
{
	Task<IEnumerable<Identity<int>>> GetAccountTypesId(
		string? name,
		decimal? baseFeeFrom,
		decimal? baseFeeTo,
		int? paymentDueDaysFrom,
		int? paymentDueDaysTo,
		int? pagingSkip,
		int? pagingLimit,
		CancellationToken cancellationToken);

	Task<int> GetAccountTypesTotal(
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
