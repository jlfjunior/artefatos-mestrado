namespace Bc.CashFlow.Domain.AccountType;

public interface IAccountTypeService
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
