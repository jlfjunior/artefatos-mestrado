using Bc.CashFlow.Domain.Transaction;

namespace Bc.CashFlow.DomainTests;

[TestFixture]
public class TransactionFactoryTests
{
	[SetUp]
	public void Setup()
	{
	}

	public static IEnumerable<TestCaseData> TransactionFactoryCreateSuccessCases
	{
		get
		{
			yield return new(1, 11, 101, TransactionType.Debit, 111.01m, null, DateTime.Now, null, null);
			yield return new(2, 12, 102, TransactionType.Credit, 112.02m, null, DateTime.Now, null, null);
			yield return new(3, 13, 103, (TransactionType)0, 113.03m, null, DateTime.Now, null, null);
			yield return new(4, 14, 104, (TransactionType)1, 114.04m, null, DateTime.Now, null, null);
		}
	}

	public static IEnumerable<TestCaseData> TransactionFactoryCreateTransactionTypeOutOfRange
	{
		get { yield return new(1, 11, 101, (TransactionType)2, 111.01m, null, DateTime.Now, null, null); }
	}

	[Test]
	[TestCaseSource(nameof(TransactionFactoryCreateSuccessCases))]
	public void GivenSuccessTransactionData_WhenFactoryCreate_ThenReturnsProperTransaction(
		int id,
		int userId,
		int accountId,
		TransactionType transactionType,
		decimal amount,
		string? description,
		DateTime transactionDate,
		decimal? transactionFee,
		DateTime? projectedRepaymentDate)
	{
		// Arrange
		TransactionFactory given = new();

		// Act
		ITransaction actual = given.Create(
			id,
			userId,
			accountId,
			transactionType,
			amount,
			description,
			transactionDate,
			transactionFee,
			projectedRepaymentDate);

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.That(actual, Is.Not.Null);
				Assert.That(actual.UserId, Is.EqualTo(userId));
				Assert.That(actual.AccountId, Is.EqualTo(accountId));
				Assert.That(actual.TransactionType, Is.EqualTo(transactionType));
				Assert.That(actual.Amount, Is.EqualTo(amount));
				Assert.That(actual.Description, Is.EqualTo(description));
				Assert.That(actual.TransactionDate, Is.EqualTo(transactionDate));
				Assert.That(actual.TransactionFee, Is.EqualTo(transactionFee));
				Assert.That(actual.ProjectedRepaymentDate, Is.EqualTo(projectedRepaymentDate));
			});
	}

	[Test]
	[TestCaseSource(nameof(TransactionFactoryCreateTransactionTypeOutOfRange))]
	public void GivenInvalidTransactionTypeData_WhenFactoryCreate_ThenThrowsTransactionTypeOutOfRange(
		int id,
		int userId,
		int accountId,
		TransactionType transactionType,
		decimal amount,
		string? description,
		DateTime transactionDate,
		decimal? transactionFee,
		DateTime? projectedRepaymentDate)
	{
		// Arrange
		TransactionFactory given = new();

		// Assert
		Assert.Throws<TransactionTypeOutOfRangeException>(
			() =>
			{
				// Act
				_ = given.Create(
					id,
					userId,
					accountId,
					transactionType,
					amount,
					description,
					transactionDate,
					transactionFee,
					projectedRepaymentDate);
			});
	}
}