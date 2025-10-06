namespace Bc.CashFlow.Domain.Account;

public class AccountFactory
{
	// ReSharper disable once MemberCanBeMadeStatic.Global
	public IAccount Create(
		int id,
		int userId,
		int accountTypeId,
		string name,
		decimal initialBalance,
		decimal currentBalance,
		DateTime balanceUpdatedAt,
		DateTime createdAt)
	{
		return new AccountVo(
			id,
			userId,
			accountTypeId,
			name,
			initialBalance,
			currentBalance,
			balanceUpdatedAt,
			createdAt);
	}
}

file record AccountVo(
	int Id,
	int UserId,
	int AccountTypeId,
	string Name,
	decimal InitialBalance,
	decimal CurrentBalance,
	DateTime BalanceUpdatedAt,
	DateTime CreatedAt
) : IAccount;