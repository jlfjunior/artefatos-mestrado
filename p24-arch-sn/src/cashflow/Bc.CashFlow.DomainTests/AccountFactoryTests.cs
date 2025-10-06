using Bc.CashFlow.Domain.Account;

namespace Bc.CashFlow.DomainTests;

[TestFixture]
public class AccountFactoryTests
{
	[SetUp]
	public void Setup()
	{
	}

	public static IEnumerable<TestCaseData> AccountFactoryCreateSuccessCases
	{
		get
		{
			yield return new(1, 11, 111, "Account Cash", 1101.01m, 1201.01m, DateTime.Now.AddDays(1),
				DateTime.Now.AddDays(10));
			yield return new(2, 12, 112, "Account Credit", 1102.02m, 1202.02m, DateTime.Now.AddDays(2),
				DateTime.Now.AddDays(20));
			yield return new(3, 13, 113, "Account Debit", 1103.03m, 1203.03m, DateTime.Now.AddDays(3),
				DateTime.Now.AddDays(30));
		}
	}

	[Test]
	[TestCaseSource(nameof(AccountFactoryCreateSuccessCases))]
	public void GivenSuccessDailyReportData_WhenFactoryCreate_ThenReturnsProperDailyReport(
		int id,
		int userId,
		int accountTypeId,
		string name,
		decimal initialBalance,
		decimal currentBalance,
		DateTime balanceUpdatedAt,
		DateTime createdAt)
	{
		// Arrange
		AccountFactory given = new();

		// Act
		IAccount actual = given.Create(
			id,
			userId,
			accountTypeId,
			name,
			initialBalance,
			currentBalance,
			balanceUpdatedAt,
			createdAt);

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.That(actual, Is.Not.Null);
				Assert.That(actual.Id, Is.EqualTo(id));
				Assert.That(actual.UserId, Is.EqualTo(userId));
				Assert.That(actual.AccountTypeId, Is.EqualTo(accountTypeId));
				Assert.That(actual.Name, Is.EqualTo(name));
				Assert.That(actual.InitialBalance, Is.EqualTo(initialBalance));
				Assert.That(actual.CurrentBalance, Is.EqualTo(currentBalance));
				Assert.That(actual.BalanceUpdatedAt, Is.EqualTo(balanceUpdatedAt));
				Assert.That(actual.CreatedAt, Is.EqualTo(createdAt));
			});
	}
}
