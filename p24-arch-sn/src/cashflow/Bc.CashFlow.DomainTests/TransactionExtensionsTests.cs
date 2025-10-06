using Bc.CashFlow.Domain.Transaction;
using Moq;

namespace Bc.CashFlow.DomainTests;

[TestFixture]
public class TransactionExtensionsTests
{
	[SetUp]
	public void Setup()
	{
	}

	#region GetAdjustedAmount

	public static IEnumerable<TestCaseData> GivenGetAdjustedAmountSuccessCases
	{
		get
		{
			yield return new(TransactionType.Debit, 100m, null, -100m);
			yield return new(TransactionType.Debit, 100m, null, -100m);
			yield return new(TransactionType.Debit, 100m, 100m, -100m);
			yield return new(TransactionType.Credit, 100m, null, 100m);
			yield return new(TransactionType.Credit, 100m, 10m, 90m);
			yield return new(0, 100m, null, -100m);
			yield return new(0, 100m, null, -100m);
			yield return new(0, 100m, 100m, -100m);
			yield return new(1, 100m, null, 100m);
			yield return new(1, 100m, 10m, 90m);
		}
	}

	[TestCaseSource(nameof(GivenGetAdjustedAmountSuccessCases))]
	public void GivenGetAdjustedAmount_WhenSuccessData_ThenReturnsExpectedAdjustedAmount(
		TransactionType transactionType,
		decimal amount,
		decimal? transactionFee,
		decimal expected)
	{
		// Arrange
		Mock<ITransaction> transactionMock = new();

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var transaction = transactionMock.Object;

		transactionMock
			.Setup(t => t.TransactionType)
			.Returns(transactionType);
		transactionMock
			.Setup(t => t.Amount)
			.Returns(amount);
		transactionMock
			.Setup(t => t.TransactionFee)
			.Returns(transactionFee);

		// Act
		decimal actual = transaction
			.GetAdjustedAmount();

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void GivenGetAdjustedAmount_WhenInvalidTransactionType_ThenThrowsTransactionTypeOutOfRangeException()
	{
		// Arrange
		Mock<ITransaction> transactionMock = new();
		const TransactionType transactionType = (TransactionType)(-1);

		transactionMock
			.Setup(t => t.TransactionType)
			.Returns(transactionType);

		ITransaction transaction = transactionMock.Object;

		// Assert
		Assert.Throws<TransactionTypeOutOfRangeException>(
			() =>
			{
				// Act
				_ = transaction.GetAdjustedAmount();
			});
	}

	#endregion
}
