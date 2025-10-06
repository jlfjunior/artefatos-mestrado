using Bc.CashFlow.Domain.DailyReport;

namespace Bc.CashFlow.DomainTests;

[TestFixture]
public class DailyReportFactoryTests
{
	[SetUp]
	public void Setup()
	{
	}

	public static IEnumerable<TestCaseData> DailyReportFactoryCreateSuccessCases
	{
		get
		{
			yield return new(1, 11, DateTime.Now, 111.01m, 211.01m, 311.01m, 411.01m, DateTime.Now.AddDays(1));
			yield return new(2, null, DateTime.Now, 112.02m, 212.02m, 312.02m, 412.02m, DateTime.Now.AddDays(2));
		}
	}

	[Test]
	[TestCaseSource(nameof(DailyReportFactoryCreateSuccessCases))]
	public void GivenSuccessDailyReportData_WhenFactoryCreate_ThenReturnsProperDailyReport(
		int id,
		int? accountId,
		DateTime date,
		decimal totalDebits,
		decimal totalCredits,
		decimal totalFee,
		decimal balance,
		DateTime createdAt)
	{
		// Arrange
		DailyReportFactory given = new();

		// Act
		IDailyReport actual = given.Create(
			id,
			accountId,
			date,
			totalDebits,
			totalCredits,
			totalFee,
			balance,
			createdAt);

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.That(actual, Is.Not.Null);
				Assert.That(actual.Id, Is.EqualTo(id));
				Assert.That(actual.AccountId, Is.EqualTo(accountId));
				Assert.That(actual.Date, Is.EqualTo(date));
				Assert.That(actual.TotalDebits, Is.EqualTo(totalDebits));
				Assert.That(actual.TotalCredits, Is.EqualTo(totalCredits));
				Assert.That(actual.TotalFee, Is.EqualTo(totalFee));
				Assert.That(actual.Balance, Is.EqualTo(balance));
				Assert.That(actual.CreatedAt, Is.EqualTo(createdAt));
			});
	}
}