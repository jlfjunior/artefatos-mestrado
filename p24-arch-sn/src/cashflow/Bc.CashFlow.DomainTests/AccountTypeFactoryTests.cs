using Bc.CashFlow.Domain.AccountType;

namespace Bc.CashFlow.DomainTests;

[TestFixture]
public class AccountTypeFactoryTests
{
	[SetUp]
	public void Setup()
	{
	}

	public static IEnumerable<TestCaseData> AccountTypeFactoryCreateSuccessCases
	{
		get
		{
			yield return new(1, "Cash", 101.01m, 1);
			yield return new(2, "Credit", 102.02m, 2);
			yield return new(3, "Debit", 103.03m, 3);
		}
	}

	[Test]
	[TestCaseSource(nameof(AccountTypeFactoryCreateSuccessCases))]
	public void GivenSuccessAccountTypeData_WhenFactoryCreate_ThenReturnsProperAccountType(
		int id,
		string name,
		decimal baseFee,
		int paymentDueDays)
	{
		// Arrange
		AccountTypeFactory given = new();

		// Act
		IAccountType actual = given.Create(
			id,
			name,
			baseFee,
			paymentDueDays);

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.That(actual, Is.Not.Null);
				Assert.That(actual.Id, Is.EqualTo(id));
				Assert.That(actual.Name, Is.EqualTo(name));
				Assert.That(actual.BaseFee, Is.EqualTo(baseFee));
				Assert.That(actual.PaymentDueDays, Is.EqualTo(paymentDueDays));
			});
	}
}
