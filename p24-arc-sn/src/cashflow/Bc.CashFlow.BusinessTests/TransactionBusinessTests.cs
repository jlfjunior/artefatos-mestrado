using System.Diagnostics.CodeAnalysis;
using Bc.CashFlow.Business;
using Bc.CashFlow.Domain.Account;
using Bc.CashFlow.Domain.AccountType;
using Bc.CashFlow.Domain.DbContext;
using Bc.CashFlow.Domain.Transaction;
using Bc.CashFlow.Domain.User;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bc.CashFlow.BusinessTests;

[TestFixture]
[SuppressMessage("ReSharper", "RedundantEmptyObjectOrCollectionInitializer")]
public class TransactionBusinessTests
{
	private Mock<ILogger<TransactionBusiness>> _loggerMock;
	private Mock<ITransactionService> _transactionServiceMock;
	private Mock<IAccountService> _accountServiceMock;
	private Mock<IAccountTypeService> _accountTypeServiceMock;
	private Mock<IUserService> _userServiceMock;
	private TransactionBusiness _transactionBusiness;

	[SetUp]
	public void Setup()
	{
		_loggerMock = new();
		_transactionServiceMock = new();
		_accountServiceMock = new();
		_accountTypeServiceMock = new();
		_userServiceMock = new();
		_transactionBusiness = new(
			_loggerMock.Object,
			_transactionServiceMock.Object,
			_accountServiceMock.Object,
			_accountTypeServiceMock.Object,
			_userServiceMock.Object);
	}

	#region GetTransactionFee

	public static IEnumerable<TestCaseData> GivenGetTransactionFeeSuccessCases
	{
		get
		{
			yield return new(0.01m, TransactionType.Debit, 100m, 0m);
			yield return new(0.02m, TransactionType.Debit, 100m, 0m);
			yield return new(0.005m, TransactionType.Debit, 200m, 0m);
			yield return new(0.01m, TransactionType.Debit, 200m, 0m);
			yield return new(0.01m, TransactionType.Credit, 100m, 1m);
			yield return new(0.02m, TransactionType.Credit, 100m, 2m);
			yield return new(0.005m, TransactionType.Credit, 200m, 1m);
			yield return new(0.01m, TransactionType.Credit, 200m, 2m);
			yield return new(0.01m, 0, 100m, 0m);
			yield return new(0.02m, 0, 100m, 0m);
			yield return new(0.005m, 0, 200m, 0m);
			yield return new(0.01m, 0, 200m, 0m);
			yield return new(0.01m, 1, 100m, 1m);
			yield return new(0.02m, 1, 100m, 2m);
			yield return new(0.005m, 1, 200m, 1m);
			yield return new(0.01m, 1, 200m, 2m);
		}
	}

	[TestCaseSource(nameof(GivenGetTransactionFeeSuccessCases))]
	public void GivenGetTransactionFee_WhenSuccessData_ThenReturnsExpectedFee(
		decimal baseFee,
		TransactionType transactionType,
		decimal amount,
		decimal expected)
	{
		// Arrange
		Mock<IAccountType> accountTypeMock = new();

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var accountType = accountTypeMock.Object;

		accountTypeMock
			.Setup(at => at.BaseFee)
			.Returns(baseFee);

		// Act
		decimal actualFee = TransactionBusiness.GetTransactionFee(
			accountType,
			transactionType,
			amount);

		// Assert
		Assert.That(actualFee, Is.EqualTo(expected));
	}

	public static IEnumerable<TestCaseData> GivenGetTransactionFeeInvalidTransactionTypeCases
	{
		get
		{
			yield return new(0.005m, -1, 200m);
			yield return new(0.01m, 3, 200m);
		}
	}

	[TestCaseSource(nameof(GivenGetTransactionFeeInvalidTransactionTypeCases))]
	public void GivenGetTransactionFee_WhenInvalidTransactionType_ThenThrowsTransactionTypeOutOfRangeException(
		decimal baseFee,
		TransactionType transactionType,
		decimal amount)
	{
		// Arrange
		Mock<IAccountType> accountTypeMock = new();

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var accountType = accountTypeMock.Object;

		accountTypeMock
			.Setup(at => at.BaseFee)
			.Returns(baseFee);

		// Assert
		Assert.Throws<TransactionTypeOutOfRangeException>(
			() =>
			{
				// Act
				_ = TransactionBusiness.GetTransactionFee(
					accountType,
					transactionType,
					amount);
			});
	}

	#endregion

	#region GetProjectedRepaymentDate

	public static IEnumerable<TestCaseData> GivenGetProjectedRepaymentDateSuccessCases
	{
		get
		{
			yield return new(1, new DateTime(2024, 10, 1), new DateTime(2024, 10, 2));
			yield return new(3, new DateTime(2024, 10, 1), new DateTime(2024, 10, 4));
			yield return new(5, new DateTime(2024, 10, 1), new DateTime(2024, 10, 6));
			yield return new(1, new DateTime(2023, 12, 31), new DateTime(2024, 1, 1));
			yield return new(0, new DateTime(2024, 10, 1), new DateTime(2024, 10, 1));
			yield return new(5, new DateTime(2024, 1, 28), new DateTime(2024, 2, 2));
			yield return new(5, new DateTime(2024, 2, 25), new DateTime(2024, 3, 1));
			yield return new(5, new DateTime(2023, 2, 25), new DateTime(2023, 3, 2));
		}
	}

	[TestCaseSource(nameof(GivenGetProjectedRepaymentDateSuccessCases))]
	public void GivenGetProjectedRepaymentDate_WhenSuccessData_ThenReturnsExpectedProjectedRepaymentDate(
		int paymentDueDays,
		DateTime transactionDate,
		DateTime expected)
	{
		// Arrange
		Mock<IAccountType> accountTypeMock = new();

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var accountType = accountTypeMock.Object;

		accountTypeMock
			.Setup(at => at.PaymentDueDays)
			.Returns(paymentDueDays);

		// Act
		DateTime actual = TransactionBusiness.GetProjectedRepaymentDate(
			accountType,
			transactionDate);

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void GivenGetProjectedRepaymentDate_WhenNegativePaymentDueDays_ThenThrowsNegativePaymentDueDaysException()
	{
		// Arrange
		Mock<IAccountType> accountTypeMock = new();

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var accountType = accountTypeMock.Object;

		accountTypeMock
			.Setup(at => at.PaymentDueDays)
			.Returns(-1);

		// Assert
		Assert.Throws<NegativePaymentDueDaysException>(
			() =>
			{
				// Act
				_ = TransactionBusiness.GetProjectedRepaymentDate(
					accountType,
					DateTime.Now);
			});
	}

	#endregion

	#region GetTransactions

	public static IEnumerable<TestCaseData> GivenGetTransactionsSuccessCases
	{
		get
		{
			yield return new(null, null, null, null, null, null, null, null, null, null);
			yield return new(null, null, null, null, null, null, null, null, null, new List<ITransaction>());
			yield return new(null, null, null, null, null, null, null, null, null, new List<ITransaction> { });
			yield return new(null, null, null, null, null, null, null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object });
			yield return new(null, null, null, null, null, null, null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object, new Mock<ITransaction>().Object });

			yield return new((int?)1, null, null, null, null, null, null, null, null, null);
			yield return new((int?)2, null, null, null, null, null, null, null, null, new List<ITransaction>());
			yield return new((int?)3, null, null, null, null, null, null, null, null, new List<ITransaction> { });
			yield return new((int?)4, null, null, null, null, null, null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object });
			yield return new((int?)5, null, null, null, null, null, null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object, new Mock<ITransaction>().Object });

			yield return new((int?)1, (int?)11, null, null, null, null, null, null, null, null);
			yield return new((int?)2, (int?)12, null, null, null, null, null, null, null, new List<ITransaction>());
			yield return new((int?)3, (int?)13, null, null, null, null, null, null, null, new List<ITransaction> { });
			yield return new((int?)4, (int?)14, null, null, null, null, null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object });
			yield return new((int?)5, (int?)15, null, null, null, null, null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object, new Mock<ITransaction>().Object });

			yield return new((int?)1, (int?)11, TransactionType.Debit, null, null, null, null, null, null, null);
			yield return new((int?)2, (int?)12, TransactionType.Debit, null, null, null, null, null, null,
				new List<ITransaction>());
			yield return new((int?)3, (int?)13, TransactionType.Debit, null, null, null, null, null, null,
				new List<ITransaction> { });
			yield return new((int?)4, (int?)14, TransactionType.Debit, null, null, null, null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object });
			yield return new((int?)5, (int?)15, TransactionType.Debit, null, null, null, null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object, new Mock<ITransaction>().Object });

			yield return new((int?)1, (int?)11, TransactionType.Credit, null, null, null, null, null, null, null);
			yield return new((int?)2, (int?)12, TransactionType.Credit, null, null, null, null, null, null,
				new List<ITransaction>());
			yield return new((int?)3, (int?)13, TransactionType.Credit, null, null, null, null, null, null,
				new List<ITransaction> { });
			yield return new((int?)4, (int?)14, TransactionType.Credit, null, null, null, null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object });
			yield return new((int?)5, (int?)15, TransactionType.Credit, null, null, null, null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object, new Mock<ITransaction>().Object });

			yield return new((int?)1, (int?)11, (TransactionType?)0, null, null, null, null, null, null, null);
			yield return new((int?)2, (int?)12, (TransactionType?)0, null, null, null, null, null, null,
				new List<ITransaction>());
			yield return new((int?)3, (int?)13, (TransactionType?)0, null, null, null, null, null, null,
				new List<ITransaction> { });
			yield return new((int?)4, (int?)14, (TransactionType?)0, null, null, null, null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object });
			yield return new((int?)5, (int?)15, (TransactionType?)0, null, null, null, null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object, new Mock<ITransaction>().Object });

			yield return new((int?)1, (int?)11, (TransactionType?)1, null, null, null, null, null, null, null);
			yield return new((int?)2, (int?)12, (TransactionType?)1, null, null, null, null, null, null,
				new List<ITransaction>());
			yield return new((int?)3, (int?)13, (TransactionType?)1, null, null, null, null, null, null,
				new List<ITransaction> { });
			yield return new((int?)4, (int?)14, (TransactionType?)1, null, null, null, null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object });
			yield return new((int?)5, (int?)15, (TransactionType?)1, null, null, null, null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object, new Mock<ITransaction>().Object });

			yield return new((int?)1, (int?)11, (TransactionType?)1, (decimal?)101, null, null, null, null, null, null);
			yield return new((int?)2, (int?)12, (TransactionType?)1, (decimal?)102, null, null, null, null, null,
				new List<ITransaction>());
			yield return new((int?)3, (int?)13, (TransactionType?)1, (decimal?)103, null, null, null, null, null,
				new List<ITransaction> { });
			yield return new((int?)4, (int?)14, (TransactionType?)1, (decimal?)104, null, null, null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object });
			yield return new((int?)5, (int?)15, (TransactionType?)1, (decimal?)105, null, null, null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object, new Mock<ITransaction>().Object });

			yield return new((int?)1, (int?)11, (TransactionType?)0, (decimal?)101, (decimal?)201, null, null, null,
				null, null);
			yield return new((int?)2, (int?)12, (TransactionType?)0, (decimal?)102, (decimal?)202, null, null, null,
				null, new List<ITransaction>());
			yield return new((int?)3, (int?)13, (TransactionType?)0, (decimal?)103, (decimal?)203, null, null, null,
				null, new List<ITransaction> { });
			yield return new((int?)4, (int?)14, (TransactionType?)0, (decimal?)104, (decimal?)204, null, null, null,
				null, new List<ITransaction> { new Mock<ITransaction>().Object });
			yield return new((int?)5, (int?)15, (TransactionType?)0, (decimal?)105, (decimal?)205, null, null, null,
				null, new List<ITransaction> { new Mock<ITransaction>().Object, new Mock<ITransaction>().Object });

			yield return new((int?)1, (int?)11, null, (decimal?)101, (decimal?)201, DateTime.Now.AddDays(1), null, null,
				null, null);
			yield return new((int?)2, (int?)12, null, (decimal?)102, (decimal?)202, DateTime.Now.AddDays(2), null, null,
				null, new List<ITransaction>());
			yield return new((int?)3, (int?)13, null, (decimal?)103, (decimal?)203, DateTime.Now.AddDays(3), null, null,
				null, new List<ITransaction> { });
			yield return new((int?)4, (int?)14, null, (decimal?)104, (decimal?)204, DateTime.Now.AddDays(4), null, null,
				null, new List<ITransaction> { new Mock<ITransaction>().Object });
			yield return new((int?)5, (int?)15, null, (decimal?)105, (decimal?)205, DateTime.Now.AddDays(5), null, null,
				null, new List<ITransaction> { new Mock<ITransaction>().Object, new Mock<ITransaction>().Object });

			yield return new((int?)1, (int?)11, TransactionType.Credit, (decimal?)101m, (decimal?)201m,
				DateTime.Now.AddDays(1), DateTime.Now.AddDays(11), null, null, null);
			yield return new((int?)2, (int?)12, TransactionType.Credit, (decimal?)102m, (decimal?)202m,
				DateTime.Now.AddDays(2), DateTime.Now.AddDays(12), null, null, new List<ITransaction>());
			yield return new((int?)3, (int?)13, TransactionType.Credit, (decimal?)103m, (decimal?)203m,
				DateTime.Now.AddDays(3), DateTime.Now.AddDays(13), null, null, new List<ITransaction> { });
			yield return new((int?)4, (int?)14, TransactionType.Credit, (decimal?)104m, (decimal?)204m,
				DateTime.Now.AddDays(4), DateTime.Now.AddDays(14), null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object });
			yield return new((int?)5, (int?)15, TransactionType.Credit, (decimal?)105m, (decimal?)205m,
				DateTime.Now.AddDays(5), DateTime.Now.AddDays(15), null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object, new Mock<ITransaction>().Object });

			yield return new((int?)1, (int?)11, null, null, null, DateTime.Now.AddDays(1), null, null, null, null);
			yield return new((int?)2, (int?)12, null, null, null, DateTime.Now.AddDays(2), null, null, null,
				new List<ITransaction>());
			yield return new((int?)3, (int?)13, null, null, null, DateTime.Now.AddDays(3), null, null, null,
				new List<ITransaction> { });
			yield return new((int?)4, (int?)14, null, null, null, DateTime.Now.AddDays(4), null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object });
			yield return new((int?)5, (int?)15, null, null, null, DateTime.Now.AddDays(5), null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object, new Mock<ITransaction>().Object });

			yield return new(null, (int?)11, (TransactionType?)1, null, (decimal?)101m, DateTime.Now.AddDays(1),
				DateTime.Now.AddDays(11), null, null, null);
			yield return new(null, (int?)12, (TransactionType?)1, null, (decimal?)102m, DateTime.Now.AddDays(2),
				DateTime.Now.AddDays(12), null, null, new List<ITransaction>());
			yield return new(null, (int?)13, (TransactionType?)1, null, (decimal?)103m, DateTime.Now.AddDays(3),
				DateTime.Now.AddDays(13), null, null, new List<ITransaction> { });
			yield return new(null, (int?)14, (TransactionType?)1, null, (decimal?)104m, DateTime.Now.AddDays(4),
				DateTime.Now.AddDays(14), null, null, new List<ITransaction> { new Mock<ITransaction>().Object });
			yield return new(null, (int?)15, (TransactionType?)1, null, (decimal?)105m, DateTime.Now.AddDays(5),
				DateTime.Now.AddDays(15), null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object, new Mock<ITransaction>().Object });

			yield return new(null, (int?)11, TransactionType.Debit, (decimal?)101m, null, DateTime.Now.AddDays(1),
				DateTime.Now.AddDays(11), DateTime.Now.AddDays(1001), null, null);
			yield return new(null, (int?)12, TransactionType.Debit, (decimal?)102m, null, DateTime.Now.AddDays(2),
				DateTime.Now.AddDays(12), DateTime.Now.AddDays(1002), null, new List<ITransaction>());
			yield return new(null, (int?)13, TransactionType.Debit, (decimal?)103m, null, DateTime.Now.AddDays(3),
				DateTime.Now.AddDays(13), DateTime.Now.AddDays(1003), null, new List<ITransaction> { });
			yield return new(null, (int?)14, TransactionType.Debit, (decimal?)104m, null, DateTime.Now.AddDays(4),
				DateTime.Now.AddDays(14), DateTime.Now.AddDays(1004), null,
				new List<ITransaction> { new Mock<ITransaction>().Object });
			yield return new(null, (int?)15, TransactionType.Debit, (decimal?)105m, null, DateTime.Now.AddDays(5),
				DateTime.Now.AddDays(15), DateTime.Now.AddDays(1005), null,
				new List<ITransaction> { new Mock<ITransaction>().Object, new Mock<ITransaction>().Object });

			yield return new(null, (int?)11, null, null, (decimal?)101m, DateTime.Now.AddDays(1),
				DateTime.Now.AddDays(11), DateTime.Now.AddDays(1001), DateTime.Now.AddDays(2001), null);
			yield return new(null, (int?)12, null, null, (decimal?)102m, DateTime.Now.AddDays(2),
				DateTime.Now.AddDays(12), DateTime.Now.AddDays(1002), DateTime.Now.AddDays(2002),
				new List<ITransaction>());
			yield return new(null, (int?)13, null, null, (decimal?)103m, DateTime.Now.AddDays(3),
				DateTime.Now.AddDays(13), DateTime.Now.AddDays(1003), DateTime.Now.AddDays(2003),
				new List<ITransaction> { });
			yield return new(null, (int?)14, null, null, (decimal?)104m, DateTime.Now.AddDays(4),
				DateTime.Now.AddDays(14), DateTime.Now.AddDays(1004), DateTime.Now.AddDays(2004),
				new List<ITransaction> { new Mock<ITransaction>().Object });
			yield return new(null, (int?)15, null, null, (decimal?)105m, DateTime.Now.AddDays(5),
				DateTime.Now.AddDays(15), DateTime.Now.AddDays(1005), DateTime.Now.AddDays(2005),
				new List<ITransaction> { new Mock<ITransaction>().Object, new Mock<ITransaction>().Object });

			yield return new(null, null, null, null, (decimal?)101m, DateTime.Now.AddDays(1), DateTime.Now.AddDays(11),
				null, DateTime.Now.AddDays(2001), null);
			yield return new(null, null, null, null, (decimal?)102m, DateTime.Now.AddDays(2), DateTime.Now.AddDays(12),
				null, DateTime.Now.AddDays(2002), new List<ITransaction>());
			yield return new(null, null, null, null, (decimal?)103m, DateTime.Now.AddDays(3), DateTime.Now.AddDays(13),
				null, DateTime.Now.AddDays(2003), new List<ITransaction> { });
			yield return new(null, null, null, null, (decimal?)104m, DateTime.Now.AddDays(4), DateTime.Now.AddDays(14),
				null, DateTime.Now.AddDays(2004), new List<ITransaction> { new Mock<ITransaction>().Object });
			yield return new(null, null, null, null, (decimal?)105m, DateTime.Now.AddDays(5), DateTime.Now.AddDays(15),
				null, DateTime.Now.AddDays(2005),
				new List<ITransaction> { new Mock<ITransaction>().Object, new Mock<ITransaction>().Object });
		}
	}

	[TestCaseSource(nameof(GivenGetTransactionsSuccessCases))]
	public async Task GivenGetTransactions_WhenSuccessData_ThenReturnsExpectedTransactions(
		int? userId,
		int? accountId,
		TransactionType? transactionType,
		decimal? amountFrom,
		decimal? amountTo,
		DateTime? transactionDateSince,
		DateTime? transactionDateUntil,
		DateTime? projectedRepaymentDateSince,
		DateTime? projectedRepaymentDateUntil,
		IEnumerable<ITransaction> expected)
	{
		// Arrange
		int? pagingSkip = null;
		int? pagingLimit = null;
		_transactionServiceMock
			.Setup(
				ts =>
					ts.GetTransactions(
						userId,
						accountId,
						transactionType,
						amountFrom,
						amountTo,
						transactionDateSince,
						transactionDateUntil,
						projectedRepaymentDateSince,
						projectedRepaymentDateUntil,
						pagingSkip,
						pagingLimit,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		// Act
		IEnumerable<ITransaction> actual =
			await _transactionBusiness.GetTransactions(
				userId,
				accountId,
				transactionType,
				amountFrom,
				amountTo,
				transactionDateSince,
				transactionDateUntil,
				projectedRepaymentDateSince,
				projectedRepaymentDateUntil,
				pagingSkip,
				pagingLimit,
				CancellationToken.None);

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.That(actual, Is.EqualTo(expected));
				_transactionServiceMock.Verify(
					ts =>
						ts.GetTransactions(
							userId,
							accountId,
							transactionType,
							amountFrom,
							amountTo,
							transactionDateSince,
							transactionDateUntil,
							projectedRepaymentDateSince,
							projectedRepaymentDateUntil,
							pagingSkip,
							pagingLimit,
							It.IsAny<CancellationToken>()),
					Times.Once);
			});
	}

	public static IEnumerable<TestCaseData> GivenGetTransactionsInvalidCases
	{
		get
		{
			yield return new((int?)1, (int?)11, (TransactionType?)-1, null, null, null, null, null, null, null);
			yield return new((int?)2, (int?)12, (TransactionType?)-1, null, null, null, null, null, null,
				new List<ITransaction>());
			yield return new((int?)3, (int?)13, (TransactionType?)-1, null, null, null, null, null, null,
				new List<ITransaction> { });
			yield return new((int?)4, (int?)14, (TransactionType?)-1, null, null, null, null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object });
			yield return new((int?)5, (int?)15, (TransactionType?)-1, null, null, null, null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object, new Mock<ITransaction>().Object });

			yield return new((int?)1, (int?)11, (TransactionType?)-1, (decimal?)101, null, null, null, null, null,
				null);
			yield return new((int?)2, (int?)12, (TransactionType?)-1, (decimal?)102, null, null, null, null, null,
				new List<ITransaction>());
			yield return new((int?)3, (int?)13, (TransactionType?)-1, (decimal?)103, null, null, null, null, null,
				new List<ITransaction> { });
			yield return new((int?)4, (int?)14, (TransactionType?)-1, (decimal?)104, null, null, null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object });
			yield return new((int?)5, (int?)15, (TransactionType?)-1, (decimal?)105, null, null, null, null, null,
				new List<ITransaction> { new Mock<ITransaction>().Object, new Mock<ITransaction>().Object });

			yield return new((int?)1, (int?)11, (TransactionType?)-1, (decimal?)101, (decimal?)201, null, null, null,
				null, null);
			yield return new((int?)2, (int?)12, (TransactionType?)-1, (decimal?)102, (decimal?)202, null, null, null,
				null, new List<ITransaction>());
			yield return new((int?)3, (int?)13, (TransactionType?)-1, (decimal?)103, (decimal?)203, null, null, null,
				null, new List<ITransaction> { });
			yield return new((int?)4, (int?)14, (TransactionType?)-1, (decimal?)104, (decimal?)204, null, null, null,
				null, new List<ITransaction> { new Mock<ITransaction>().Object });
			yield return new((int?)5, (int?)15, (TransactionType?)-1, (decimal?)105, (decimal?)205, null, null, null,
				null, new List<ITransaction> { new Mock<ITransaction>().Object, new Mock<ITransaction>().Object });
		}
	}

	[TestCaseSource(nameof(GivenGetTransactionsInvalidCases))]
	public void GivenGetTransactions_WhenInvalidTransactionType_ThenThrowsTransactionTypeOutOfRangeException(
		int? userId,
		int? accountId,
		TransactionType? transactionType,
		decimal? amountFrom,
		decimal? amountTo,
		DateTime? transactionDateSince,
		DateTime? transactionDateUntil,
		DateTime? projectedRepaymentDateSince,
		DateTime? projectedRepaymentDateUntil,
		IEnumerable<ITransaction> expected)
	{
		// Arrange
		int? pagingSkip = null;
		int? pagingLimit = null;
		_transactionServiceMock
			.Setup(
				ts =>
					ts.GetTransactions(
						userId,
						accountId,
						transactionType,
						amountFrom,
						amountTo,
						transactionDateSince,
						transactionDateUntil,
						projectedRepaymentDateSince,
						projectedRepaymentDateUntil,
						pagingSkip,
						pagingLimit,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		// Assert
		Assert.Throws<TransactionTypeOutOfRangeException>(
			() =>
			{
				// Act
				_ = _transactionBusiness.GetTransactions(
						userId,
						accountId,
						transactionType,
						amountFrom,
						amountTo,
						transactionDateSince,
						transactionDateUntil,
						projectedRepaymentDateSince,
						projectedRepaymentDateUntil,
						pagingSkip,
						pagingLimit,
						CancellationToken.None)
					.GetAwaiter()
					.GetResult();
			});
	}

	#endregion

	#region GetRequiredAccount

	public static IEnumerable<TestCaseData> GivenGetRequiredAccountSuccessCases
	{
		get { yield return new(1); }
	}

	[TestCaseSource(nameof(GivenGetRequiredAccountSuccessCases))]
	public async Task GivenGetRequiredAccount_WhenSuccessData_ThenReturnsExpectedAccount(
		int accountId)
	{
		// Arrange
		Mock<IAccount> expectedMock = new();

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var expected = expectedMock.Object;

		_accountServiceMock
			.Setup(
				a =>
					a.GetAccount(
						accountId,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		// Act
		IAccount actual =
			await _transactionBusiness
				.GetRequiredAccount(
					accountId,
					CancellationToken.None);

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.That(actual, Is.EqualTo(expected));
				_accountServiceMock.Verify(
					a =>
						a.GetAccount(
							accountId,
							It.IsAny<CancellationToken>()),
					Times.Once);
			});
	}

	public static IEnumerable<TestCaseData> GivenGetRequiredAccountAccountNotFoundCases
	{
		get { yield return new(1); }
	}

	[TestCaseSource(nameof(GivenGetRequiredAccountAccountNotFoundCases))]
	public void GivenGetRequiredAccount_WhenAccountNotFound_ThenThrowsAccountNotFoundException(
		int accountId)
	{
		// Arrange
		_accountServiceMock
			.Setup(
				a =>
					a.GetAccount(
						accountId,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync((IAccount?)null);

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.Throws<AccountNotFoundException>(
					() => 
					{
						// Act
						_ = _transactionBusiness
								.GetRequiredAccount(
									accountId,
									CancellationToken.None)
								.GetAwaiter()
								.GetResult();
					});
				_accountServiceMock.Verify(
					a =>
						a.GetAccount(
							accountId,
							It.IsAny<CancellationToken>()),
					Times.Once);
			});
	}

	public static IEnumerable<TestCaseData> GivenGetRequiredAccountCancellationTokenCancelRequestedCases
	{
		get { yield return new(1, new CancellationTokenSource()); }
	}

	[TestCaseSource(nameof(GivenGetRequiredAccountCancellationTokenCancelRequestedCases))]
	public async Task
		GivenGetRequiredAccount_WhenCancellationTokenRequestCancel_ThenThrowsThrowOperationCanceledException(
			int accountId,
			CancellationTokenSource cancellationTokenSource)
	{
		// Arrange
		await cancellationTokenSource.CancelAsync();

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.Throws<OperationCanceledException>(
					() =>
					{
						// Act
						_ = _transactionBusiness
							.GetRequiredAccount(
								accountId,
								cancellationTokenSource.Token)
							.GetAwaiter()
							.GetResult();
					});
			});
	}

	#endregion

	#region GetRequiredAccountType

	public static IEnumerable<TestCaseData> GivenGetRequiredAccountTypeSuccessCases
	{
		get
		{
			yield return new(11);
			yield return new(12);
			yield return new(13);
			yield return new(14);
		}
	}

	[TestCaseSource(nameof(GivenGetRequiredAccountTypeSuccessCases))]
	public async Task GivenGetRequiredAccountType_WhenSuccessData_ThenReturnsExpectedAccountType(
		int accountTypeId)
	{
		// Arrange
		Mock<IAccountType> expectedMock = new();

		expectedMock.Setup(e => e.Id).Returns(accountTypeId);

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var expected = expectedMock.Object;
		Mock<IAccount> accountMock = new();

		accountMock.Setup(e => e.AccountTypeId).Returns(accountTypeId);

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var account = accountMock.Object;

		_accountTypeServiceMock
			.Setup(
				a =>
					a.GetAccountType(
						accountTypeId,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		// Act
		IAccountType actual =
			await _transactionBusiness
				.GetRequiredAccountType(
					account,
					CancellationToken.None);

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.That(actual, Is.EqualTo(expected));
				_accountTypeServiceMock.Verify(
					a =>
						a.GetAccountType(
							accountTypeId,
							It.IsAny<CancellationToken>()),
					Times.Once);
			});
	}

	public static IEnumerable<TestCaseData> GivenGetRequiredAccountTypeInvalidCases
	{
		get
		{
			yield return new(11);
			yield return new(12);
			yield return new(13);
			yield return new(14);
		}
	}

	[TestCaseSource(nameof(GivenGetRequiredAccountTypeInvalidCases))]
	public void GivenGetRequiredAccountType_WhenSuccessDataReturnsNull_ThenThrowsAccountTypeNotFoundException(
		int accountTypeId)
	{
		// Arrange
		Mock<IAccount> accountMock = new();

		accountMock.Setup(e => e.AccountTypeId).Returns(accountTypeId);

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var account = accountMock.Object;

		_accountTypeServiceMock
			.Setup(
				a =>
					a.GetAccountType(
						accountTypeId,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync((IAccountType?)null);

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.Throws<AccountTypeNotFoundException>(
					() =>
					{
						// Act
						_ = _transactionBusiness
							.GetRequiredAccountType(
								account,
								CancellationToken.None)
							.GetAwaiter()
							.GetResult();
					});
				_accountTypeServiceMock.Verify(
					a =>
						a.GetAccountType(
							accountTypeId,
							It.IsAny<CancellationToken>()),
					Times.Once);
			});
	}

	public static IEnumerable<TestCaseData> GivenGetRequiredAccountTypeCancellationTokenCancelRequestedCases
	{
		get { yield return new(1, new CancellationTokenSource()); }
	}

	[TestCaseSource(nameof(GivenGetRequiredAccountTypeCancellationTokenCancelRequestedCases))]
	public async Task
		GivenGetRequiredAccountType_WhenCancellationTokenRequestCancel_ThenThrowsThrowOperationCanceledException(
			int accountTypeId,
			CancellationTokenSource cancellationTokenSource)
	{
		// Arrange
		Mock<IAccount> accountMock = new();

		accountMock.Setup(e => e.AccountTypeId).Returns(accountTypeId);

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var account = accountMock.Object;

		await cancellationTokenSource.CancelAsync();

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.Throws<OperationCanceledException>(
					() =>
					{
						// Act
						_ = _transactionBusiness
							.GetRequiredAccountType(
								account,
								cancellationTokenSource.Token)
							.GetAwaiter()
							.GetResult();
					});
			});
	}

	#endregion

	#region GetRequiredUser

	public static IEnumerable<TestCaseData> GivenGetRequiredUserSuccessCases
	{
		get
		{
			yield return new(11);
			yield return new(12);
			yield return new(13);
			yield return new(14);
		}
	}

	[TestCaseSource(nameof(GivenGetRequiredUserSuccessCases))]
	public async Task GivenGetRequiredUser_WhenSuccessData_ThenReturnsExpectedUser(
		int userId)
	{
		// Arrange
		Mock<IUser> expectedMock = new();

		expectedMock.Setup(e => e.Id).Returns(userId);

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var expected = expectedMock.Object;

		_userServiceMock
			.Setup(
				a =>
					a.GetSingleUser(
						userId,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		// Act
		IUser actual =
			await _transactionBusiness
				.GetRequiredUser(
					userId,
					CancellationToken.None);

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.That(actual, Is.EqualTo(expected));
				_userServiceMock.Verify(
					a =>
						a.GetSingleUser(
							userId,
							It.IsAny<CancellationToken>()),
					Times.Once);
			});
	}

	public static IEnumerable<TestCaseData> GivenGetRequiredUserInvalidCases
	{
		get
		{
			yield return new(11);
			yield return new(12);
			yield return new(13);
			yield return new(14);
		}
	}

	[TestCaseSource(nameof(GivenGetRequiredUserInvalidCases))]
	public void GivenGetRequiredUser_WhenSuccessDataReturnsNull_ThenThrowsUserNotFoundException(
		int userId)
	{
		// Arrange
		_userServiceMock
			.Setup(
				a =>
					a.GetSingleUser(
						userId,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync((IUser?)null);

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.Throws<UserNotFoundException>(
					() =>
					{
						// Act
						_ = _transactionBusiness
							.GetRequiredUser(
								userId,
								CancellationToken.None)
							.GetAwaiter()
							.GetResult();
					});
				_userServiceMock.Verify(
					a =>
						a.GetSingleUser(
							userId,
							It.IsAny<CancellationToken>()),
					Times.Once);
			});
	}

	public static IEnumerable<TestCaseData> GivenGetRequiredUserCancellationTokenCancelRequestedCases
	{
		get { yield return new(1, new CancellationTokenSource()); }
	}

	[TestCaseSource(nameof(GivenGetRequiredUserCancellationTokenCancelRequestedCases))]
	public void
		GivenGetRequiredUser_WhenCancellationTokenRequestCancel_ThenThrowsThrowOperationCanceledException(
			int userId,
			CancellationTokenSource cancellationTokenSource)
	{
		// Arrange
		cancellationTokenSource.Cancel();

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.Throws<OperationCanceledException>(
					() =>
					{
						// Act
						_ = _transactionBusiness
							.GetRequiredUser(
								userId,
								cancellationTokenSource.Token)
							.GetAwaiter()
							.GetResult();
					});
			});
	}

	#endregion

	#region UpdateAccountBalance

	public static IEnumerable<TestCaseData> GivenUpdateAccountBalanceSuccessCases
	{
		get
		{
			yield return new(1, 11, TransactionType.Debit, 101m, 1.001m);
			yield return new(2, 12, TransactionType.Credit, 102m, 1.002m);
			yield return new(3, 13, 0, 103m, 1.003m);
			yield return new(4, 14, 1, 104m, 1.004m);
		}
	}

	[TestCaseSource(nameof(GivenUpdateAccountBalanceSuccessCases))]
	public async Task GivenUpdateAccountBalance_WhenSuccessData_ThenProducesNoErrors(
		int transactionId,
		int accountId,
		TransactionType transactionType,
		decimal amount,
		decimal transactionFee)
	{
		// Arrange
		Mock<ITransaction> transactionMock = new();

		transactionMock.Setup(t => t.AccountId).Returns(accountId);
		transactionMock.Setup(t => t.TransactionType).Returns(transactionType);
		transactionMock.Setup(t => t.Amount).Returns(amount);
		transactionMock.Setup(t => t.TransactionFee).Returns(transactionFee);

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var transaction = transactionMock.Object;

		_transactionServiceMock
			.Setup(
				ts =>
					ts.GetTransaction(
						transactionId,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync(transaction);

		decimal adjustedAmount = transaction.GetAdjustedAmount();

		_accountServiceMock
			.Setup(
				a =>
					a.UpdateBalance(
						accountId,
						adjustedAmount,
						It.IsAny<CancellationToken>()));

		// Act
		await _transactionBusiness.UpdateAccountBalance(
			transactionId,
			CancellationToken.None);

		// Assert
		Assert.Multiple(
			() =>
			{
				_transactionServiceMock.Verify(
					a =>
						a.GetTransaction(
							transactionId,
							It.IsAny<CancellationToken>()),
					Times.Once);
				_accountServiceMock.Verify(
					a =>
						a.UpdateBalance(
							accountId,
							adjustedAmount,
							It.IsAny<CancellationToken>()),
					Times.Once);
			});
	}

	public static IEnumerable<TestCaseData> GivenUpdateAccountBalanceInvalidCases
	{
		get
		{
			yield return new(1);
			yield return new(2);
			yield return new(3);
			yield return new(4);
		}
	}

	[TestCaseSource(nameof(GivenUpdateAccountBalanceInvalidCases))]
	public void GivenUpdateAccountBalance_WhenTransactionNotFound_ThenThrowsTransactionNotFoundException(
		int transactionId)
	{
		// Arrange
		_transactionServiceMock
			.Setup(
				ts =>
					ts.GetTransaction(
						transactionId,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync((ITransaction?)null);

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.Throws<TransactionNotFoundException>(
					() =>
					{
						// Act
						_transactionBusiness.UpdateAccountBalance(
								transactionId,
								CancellationToken.None)
							.GetAwaiter()
							.GetResult();
					});
				_transactionServiceMock.Verify(
					a =>
						a.GetTransaction(
							transactionId,
							It.IsAny<CancellationToken>()),
					Times.Once);
			});
	}

	public static IEnumerable<TestCaseData> GivenUpdateAccountBalanceCancellationTokenCancelRequestedCases
	{
		get
		{
			yield return new(1);
			yield return new(2);
			yield return new(3);
			yield return new(4);
		}
	}

	[TestCaseSource(nameof(GivenUpdateAccountBalanceCancellationTokenCancelRequestedCases))]
	public void GivenUpdateAccountBalance_WhenCancellationTokenCancelRequested_ThenThrowsOperationCanceledException(
		int transactionId)
	{
		// Arrange
		CancellationTokenSource cancellationTokenSource = new();

		cancellationTokenSource.Cancel();

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.Throws<OperationCanceledException>(
					() =>
					{
						// Act
						_transactionBusiness.UpdateAccountBalance(
								transactionId,
								cancellationTokenSource.Token)
							.GetAwaiter()
							.GetResult();
					});
			});
	}

	#endregion

	#region CreateRequiredTransaction

	public static IEnumerable<TestCaseData> GivenCreateRequiredTransactionSuccessCases
	{
		get
		{
			yield return new(101, 1011, 10101m, (int?)101001, TransactionType.Debit, 101101m, "",
				DateTime.Now.AddDays(101), 1011001);
			yield return new(102, 1012, 10102m, (int?)101002, TransactionType.Credit, 101102m, "",
				DateTime.Now.AddDays(102), 1011002);
			yield return new(103, 1013, 10103m, null, TransactionType.Debit, 101103m, "string3 desc",
				DateTime.Now.AddDays(103), 1011003);
			yield return new(104, 1014, 10104m, null, TransactionType.Credit, 101104m, "string4 desc",
				DateTime.Now.AddDays(104), 1011004);
			yield return new(105, 1015, 10105m, null, TransactionType.Debit, 101105m, null, DateTime.Now.AddDays(105),
				1011005);
			yield return new(106, 1016, 10106m, null, TransactionType.Credit, 101106m, null, DateTime.Now.AddDays(106),
				1011006);
			yield return new(107, 1017, 10107m, (int?)101007, TransactionType.Debit, 101107m, null,
				DateTime.Now.AddDays(107), 1011007);
			yield return new(108, 1018, 10108m, (int?)101008, TransactionType.Credit, 101108m, null,
				DateTime.Now.AddDays(108), 1011008);

			yield return new(201, 2011, 20101m, (int?)201001, 0, 201101m, "", DateTime.Now.AddDays(201), 2011001);
			yield return new(202, 2012, 20102m, (int?)201002, 1, 201102m, "", DateTime.Now.AddDays(202), 2011002);
			yield return new(203, 2013, 20103m, null, 0, 201103m, "string3 desc", DateTime.Now.AddDays(203), 2011003);
			yield return new(204, 2014, 20104m, null, 1, 201104m, "string4 desc", DateTime.Now.AddDays(204), 2011004);
			yield return new(205, 2015, 20105m, null, 0, 201105m, null, DateTime.Now.AddDays(205), 2011005);
			yield return new(206, 2016, 20106m, null, 1, 201106m, null, DateTime.Now.AddDays(206), 2011006);
			yield return new(207, 2017, 20107m, (int?)201007, 0, 201107m, null, DateTime.Now.AddDays(207), 2011007);
			yield return new(208, 2018, 20108m, (int?)201008, 1, 201108m, null, DateTime.Now.AddDays(208), 2011008);
		}
	}

	[TestCaseSource(nameof(GivenCreateRequiredTransactionSuccessCases))]
	public async Task GivenCreateRequiredTransaction_WhenSuccessData_ThenReturnsExpectedIdentity(
		int accountId,
		int accountTypeId,
		decimal baseFee,
		int? userId,
		TransactionType transactionType,
		decimal amount,
		string? description,
		DateTime transactionDate,
		int? expectedId)
	{
		// Arrange
		Mock<IAccountType> accountTypeMock = new();

		accountTypeMock.Setup(e => e.Id).Returns(accountTypeId);
		accountTypeMock.Setup(e => e.BaseFee).Returns(baseFee);

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var accountType = accountTypeMock.Object;

		Mock<IAccount> accountMock = new();

		accountMock.Setup(e => e.Id).Returns(accountId);
		accountMock.Setup(e => e.AccountTypeId).Returns(accountTypeId);

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var account = accountMock.Object;

		_accountTypeServiceMock
			.Setup(
				a =>
					a.GetAccountType(
						accountTypeId,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync((IAccountType?)accountType);

		decimal transactionFee = TransactionBusiness.GetTransactionFee(
			accountType,
			transactionType,
			amount);
		DateTime projectedRepaymentDate = TransactionBusiness.GetProjectedRepaymentDate(
			accountType,
			transactionDate);
		Identity<int>? expected = null;

		if (expectedId is not null)
		{
			expected =
				new()
				{
					Value = expectedId.Value
				};
		}

		_transactionServiceMock
			.Setup(
				ts =>
					ts.CreateTransaction(
						userId,
						accountId,
						transactionType,
						amount,
						description,
						transactionDate,
						transactionFee,
						projectedRepaymentDate,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		IUser? user = null;

		if (userId is not null)
		{
			Mock<IUser> userMock = new();

			userMock.Setup(u => u.Id).Returns(userId.Value);

			user = userMock.Object;
		}

		// Act
		Identity<int>? actual =
			await _transactionBusiness
				.CreateRequiredTransaction(
					user,
					account,
					transactionType,
					amount,
					description,
					transactionDate,
					CancellationToken.None);

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.That(actual, Is.EqualTo(expected));
				_accountTypeServiceMock
					.Verify(
						a =>
							a.GetAccountType(
								accountTypeId,
								It.IsAny<CancellationToken>()),
						Times.Once);
				_transactionServiceMock
					.Verify(
						ts =>
							ts.CreateTransaction(
								userId,
								accountId,
								transactionType,
								amount,
								description,
								transactionDate,
								transactionFee,
								projectedRepaymentDate,
								It.IsAny<CancellationToken>()),
						Times.Once);
			});
	}

	public static IEnumerable<TestCaseData> GivenCreateRequiredTransactionAccountTypeInvalidDataCases
	{
		get
		{
			yield return new(101, 1011, 10101m, (int?)101001, TransactionType.Debit, 101101m, "",
				DateTime.Now.AddDays(101), 1011001);
			yield return new(102, 1012, 10102m, (int?)101002, TransactionType.Credit, 101102m, "",
				DateTime.Now.AddDays(102), 1011002);
			yield return new(103, 1013, 10103m, null, TransactionType.Debit, 101103m, "string3 desc",
				DateTime.Now.AddDays(103), 1011003);
			yield return new(104, 1014, 10104m, null, TransactionType.Credit, 101104m, "string4 desc",
				DateTime.Now.AddDays(104), 1011004);
			yield return new(105, 1015, 10105m, null, TransactionType.Debit, 101105m, null, DateTime.Now.AddDays(105),
				1011005);
			yield return new(106, 1016, 10106m, null, TransactionType.Credit, 101106m, null, DateTime.Now.AddDays(106),
				1011006);
			yield return new(107, 1017, 10107m, (int?)101007, TransactionType.Debit, 101107m, null,
				DateTime.Now.AddDays(107), 1011007);
			yield return new(108, 1018, 10108m, (int?)101008, TransactionType.Credit, 101108m, null,
				DateTime.Now.AddDays(108), 1011008);

			yield return new(201, 2011, 20101m, (int?)201001, 0, 201101m, "", DateTime.Now.AddDays(201), 2011001);
			yield return new(202, 2012, 20102m, (int?)201002, 1, 201102m, "", DateTime.Now.AddDays(202), 2011002);
			yield return new(203, 2013, 20103m, null, 0, 201103m, "string3 desc", DateTime.Now.AddDays(203), 2011003);
			yield return new(204, 2014, 20104m, null, 1, 201104m, "string4 desc", DateTime.Now.AddDays(204), 2011004);
			yield return new(205, 2015, 20105m, null, 0, 201105m, null, DateTime.Now.AddDays(205), 2011005);
			yield return new(206, 2016, 20106m, null, 1, 201106m, null, DateTime.Now.AddDays(206), 2011006);
			yield return new(207, 2017, 20107m, (int?)201007, 0, 201107m, null, DateTime.Now.AddDays(207), 2011007);
			yield return new(208, 2018, 20108m, (int?)201008, 1, 201108m, null, DateTime.Now.AddDays(208), 2011008);
		}
	}

	[TestCaseSource(nameof(GivenCreateRequiredTransactionAccountTypeInvalidDataCases))]
	public void GivenCreateRequiredTransaction_WhenAccountTypeInvalidData_ThenThrowsAccountTypeNotFoundException(
		int accountId,
		int accountTypeId,
		decimal baseFee,
		int? userId,
		TransactionType transactionType,
		decimal amount,
		string? description,
		DateTime transactionDate,
		int? expectedId)
	{
		// Arrange
		Mock<IAccountType> accountTypeMock = new();

		accountTypeMock.Setup(e => e.Id).Returns(accountTypeId);
		accountTypeMock.Setup(e => e.BaseFee).Returns(baseFee);

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var accountType = accountTypeMock.Object;

		Mock<IAccount> accountMock = new();

		accountMock.Setup(e => e.Id).Returns(accountId);
		accountMock.Setup(e => e.AccountTypeId).Returns(accountTypeId);

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var account = accountMock.Object;

		_accountTypeServiceMock
			.Setup(
				a =>
					a.GetAccountType(
						accountTypeId,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync((IAccountType?)null);

		IUser? user = null;

		if (userId is not null)
		{
			Mock<IUser> userMock = new();

			userMock.Setup(u => u.Id).Returns(userId.Value);

			user = userMock.Object;
		}

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.Throws<AccountTypeNotFoundException>(
					() =>
					{
						// Act
						_ = _transactionBusiness
							.CreateRequiredTransaction(
								user,
								account,
								transactionType,
								amount,
								description,
								transactionDate,
								CancellationToken.None)
							.GetAwaiter()
							.GetResult();
					});
			});
	}

	public static IEnumerable<TestCaseData> GivenCreateRequiredTransactionReturnsNullCases
	{
		get
		{
			yield return new(101, 1011, 10101m, (int?)101001, TransactionType.Debit, 101101m, "",
				DateTime.Now.AddDays(101), 1011001);
			yield return new(102, 1012, 10102m, (int?)101002, TransactionType.Credit, 101102m, "",
				DateTime.Now.AddDays(102), 1011002);
			yield return new(103, 1013, 10103m, null, TransactionType.Debit, 101103m, "string3 desc",
				DateTime.Now.AddDays(103), 1011003);
			yield return new(104, 1014, 10104m, null, TransactionType.Credit, 101104m, "string4 desc",
				DateTime.Now.AddDays(104), 1011004);
			yield return new(105, 1015, 10105m, null, TransactionType.Debit, 101105m, null, DateTime.Now.AddDays(105),
				1011005);
			yield return new(106, 1016, 10106m, null, TransactionType.Credit, 101106m, null, DateTime.Now.AddDays(106),
				1011006);
			yield return new(107, 1017, 10107m, (int?)101007, TransactionType.Debit, 101107m, null,
				DateTime.Now.AddDays(107), 1011007);
			yield return new(108, 1018, 10108m, (int?)101008, TransactionType.Credit, 101108m, null,
				DateTime.Now.AddDays(108), 1011008);

			yield return new(201, 2011, 20101m, (int?)201001, 0, 201101m, "", DateTime.Now.AddDays(201), 2011001);
			yield return new(202, 2012, 20102m, (int?)201002, 1, 201102m, "", DateTime.Now.AddDays(202), 2011002);
			yield return new(203, 2013, 20103m, null, 0, 201103m, "string3 desc", DateTime.Now.AddDays(203), 2011003);
			yield return new(204, 2014, 20104m, null, 1, 201104m, "string4 desc", DateTime.Now.AddDays(204), 2011004);
			yield return new(205, 2015, 20105m, null, 0, 201105m, null, DateTime.Now.AddDays(205), 2011005);
			yield return new(206, 2016, 20106m, null, 1, 201106m, null, DateTime.Now.AddDays(206), 2011006);
			yield return new(207, 2017, 20107m, (int?)201007, 0, 201107m, null, DateTime.Now.AddDays(207), 2011007);
			yield return new(208, 2018, 20108m, (int?)201008, 1, 201108m, null, DateTime.Now.AddDays(208), 2011008);
		}
	}

	[TestCaseSource(nameof(GivenCreateRequiredTransactionReturnsNullCases))]
	public void
		GivenCreateRequiredTransaction_WhenReturnsNull_ThenThrowsTransactionCreationReturnedNullIdentityException(
			int accountId,
			int accountTypeId,
			decimal baseFee,
			int? userId,
			TransactionType transactionType,
			decimal amount,
			string? description,
			DateTime transactionDate,
			int? expectedId)
	{
		// Arrange
		Mock<IAccountType> accountTypeMock = new();

		accountTypeMock.Setup(e => e.Id).Returns(accountTypeId);
		accountTypeMock.Setup(e => e.BaseFee).Returns(baseFee);

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var accountType = accountTypeMock.Object;

		Mock<IAccount> accountMock = new();

		accountMock.Setup(e => e.Id).Returns(accountId);
		accountMock.Setup(e => e.AccountTypeId).Returns(accountTypeId);

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var account = accountMock.Object;

		_accountTypeServiceMock
			.Setup(
				a =>
					a.GetAccountType(
						accountTypeId,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync((IAccountType?)accountType);

		decimal transactionFee = TransactionBusiness.GetTransactionFee(
			accountType,
			transactionType,
			amount);
		DateTime projectedRepaymentDate = TransactionBusiness.GetProjectedRepaymentDate(
			accountType,
			transactionDate);
		Identity<int>? expected = null;

		_transactionServiceMock
			.Setup(
				ts =>
					ts.CreateTransaction(
						userId,
						accountId,
						transactionType,
						amount,
						description,
						transactionDate,
						transactionFee,
						projectedRepaymentDate,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		IUser? user = null;

		if (userId is not null)
		{
			Mock<IUser> userMock = new();

			userMock.Setup(u => u.Id).Returns(userId.Value);

			user = userMock.Object;
		}

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.Throws<TransactionCreationReturnedNullIdentityException>(
					() =>
					{
						// Act
						_ = _transactionBusiness
							.CreateRequiredTransaction(
								user,
								account,
								transactionType,
								amount,
								description,
								transactionDate,
								CancellationToken.None)
							.GetAwaiter()
							.GetResult();
					});
				_accountTypeServiceMock
					.Verify(
						a =>
							a.GetAccountType(
								accountTypeId,
								It.IsAny<CancellationToken>()),
						Times.Once);
				_transactionServiceMock
					.Verify(
						ts =>
							ts.CreateTransaction(
								userId,
								accountId,
								transactionType,
								amount,
								description,
								transactionDate,
								transactionFee,
								projectedRepaymentDate,
								It.IsAny<CancellationToken>()),
						Times.Once);
			});
	}

	public static IEnumerable<TestCaseData> GivenCreateRequiredTransactionCancellationTokenCancelRequestedCases
	{
		get
		{
			yield return new(101, 1011, 10101m, (int?)101001, TransactionType.Debit, 101101m, "",
				DateTime.Now.AddDays(101), 1011001);
			yield return new(102, 1012, 10102m, (int?)101002, TransactionType.Credit, 101102m, "",
				DateTime.Now.AddDays(102), 1011002);
			yield return new(103, 1013, 10103m, null, TransactionType.Debit, 101103m, "string3 desc",
				DateTime.Now.AddDays(103), 1011003);
			yield return new(104, 1014, 10104m, null, TransactionType.Credit, 101104m, "string4 desc",
				DateTime.Now.AddDays(104), 1011004);
			yield return new(105, 1015, 10105m, null, TransactionType.Debit, 101105m, null, DateTime.Now.AddDays(105),
				1011005);
			yield return new(106, 1016, 10106m, null, TransactionType.Credit, 101106m, null, DateTime.Now.AddDays(106),
				1011006);
			yield return new(107, 1017, 10107m, (int?)101007, TransactionType.Debit, 101107m, null,
				DateTime.Now.AddDays(107), 1011007);
			yield return new(108, 1018, 10108m, (int?)101008, TransactionType.Credit, 101108m, null,
				DateTime.Now.AddDays(108), 1011008);

			yield return new(201, 2011, 20101m, (int?)201001, 0, 201101m, "", DateTime.Now.AddDays(201), 2011001);
			yield return new(202, 2012, 20102m, (int?)201002, 1, 201102m, "", DateTime.Now.AddDays(202), 2011002);
			yield return new(203, 2013, 20103m, null, 0, 201103m, "string3 desc", DateTime.Now.AddDays(203), 2011003);
			yield return new(204, 2014, 20104m, null, 1, 201104m, "string4 desc", DateTime.Now.AddDays(204), 2011004);
			yield return new(205, 2015, 20105m, null, 0, 201105m, null, DateTime.Now.AddDays(205), 2011005);
			yield return new(206, 2016, 20106m, null, 1, 201106m, null, DateTime.Now.AddDays(206), 2011006);
			yield return new(207, 2017, 20107m, (int?)201007, 0, 201107m, null, DateTime.Now.AddDays(207), 2011007);
			yield return new(208, 2018, 20108m, (int?)201008, 1, 201108m, null, DateTime.Now.AddDays(208), 2011008);
		}
	}

	[TestCaseSource(nameof(GivenCreateRequiredTransactionCancellationTokenCancelRequestedCases))]
	public void
		GivenCreateRequiredTransaction_WhenCancellationTokenCancelRequested_ThenThrowsOperationCanceledException(
			int accountId,
			int accountTypeId,
			decimal baseFee,
			int? userId,
			TransactionType transactionType,
			decimal amount,
			string? description,
			DateTime transactionDate,
			int? expectedId)
	{
		Mock<IAccount> accountMock = new();

		accountMock.Setup(e => e.Id).Returns(accountId);
		accountMock.Setup(e => e.AccountTypeId).Returns(accountTypeId);

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var account = accountMock.Object;

		IUser? user = null;

		if (userId is not null)
		{
			Mock<IUser> userMock = new();

			userMock.Setup(u => u.Id).Returns(userId.Value);

			user = userMock.Object;
		}

		CancellationTokenSource cancellationTokenSource = new();

		cancellationTokenSource.Cancel();

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.Throws<OperationCanceledException>(
					() =>
					{
						// Act
						_ = _transactionBusiness
							.CreateRequiredTransaction(
								user,
								account,
								transactionType,
								amount,
								description,
								transactionDate,
								cancellationTokenSource.Token)
							.GetAwaiter()
							.GetResult();
					});
			});
	}

	#endregion

	#region CreateTransaction

	public static IEnumerable<TestCaseData> GivenCreateTransactionSuccessCases
	{
		get
		{
			yield return new(101, 1011, 10101m, 101001, TransactionType.Debit, 101101m, "", DateTime.Now.AddDays(101),
				1011001);
			yield return new(102, 1012, 10102m, 101002, TransactionType.Credit, 101102m, "", DateTime.Now.AddDays(102),
				1011002);
			yield return new(103, 1013, 10103m, 101003, TransactionType.Debit, 101103m, "string3 desc",
				DateTime.Now.AddDays(103), 1011003);
			yield return new(104, 1014, 10104m, 101004, TransactionType.Credit, 101104m, "string4 desc",
				DateTime.Now.AddDays(104), 1011004);
			yield return new(105, 1015, 10105m, 101005, TransactionType.Debit, 101105m, null, DateTime.Now.AddDays(105),
				1011005);
			yield return new(106, 1016, 10106m, 101006, TransactionType.Credit, 101106m, null,
				DateTime.Now.AddDays(106), 1011006);
			yield return new(107, 1017, 10107m, 101007, TransactionType.Debit, 101107m, null, DateTime.Now.AddDays(107),
				1011007);
			yield return new(108, 1018, 10108m, 101008, TransactionType.Credit, 101108m, null,
				DateTime.Now.AddDays(108), 1011008);

			yield return new(201, 2011, 20101m, 201001, 0, 201101m, "", DateTime.Now.AddDays(201), 2011001);
			yield return new(202, 2012, 20102m, 201002, 1, 201102m, "", DateTime.Now.AddDays(202), 2011002);
			yield return new(203, 2013, 20103m, 201003, 0, 201103m, "string3 desc", DateTime.Now.AddDays(203), 2011003);
			yield return new(204, 2014, 20104m, 201004, 1, 201104m, "string4 desc", DateTime.Now.AddDays(204), 2011004);
			yield return new(205, 2015, 20105m, 201005, 0, 201105m, null, DateTime.Now.AddDays(205), 2011005);
			yield return new(206, 2016, 20106m, 201006, 1, 201106m, null, DateTime.Now.AddDays(206), 2011006);
			yield return new(207, 2017, 20107m, 201007, 0, 201107m, null, DateTime.Now.AddDays(207), 2011007);
			yield return new(208, 2018, 20108m, 201008, 1, 201108m, null, DateTime.Now.AddDays(208), 2011008);
		}
	}

	[TestCaseSource(nameof(GivenCreateTransactionSuccessCases))]
	public async Task GivenCreateTransaction_WhenSuccessData_ThenReturnsExpectedIdentity(
		int accountId,
		int accountTypeId,
		decimal baseFee,
		int userId,
		TransactionType transactionType,
		decimal amount,
		string? description,
		DateTime transactionDate,
		int expectedId)
	{
		// Arrange
		Mock<IAccountType> accountTypeMock = new();

		accountTypeMock.Setup(e => e.Id).Returns(accountTypeId);
		accountTypeMock.Setup(e => e.BaseFee).Returns(baseFee);

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var accountType = accountTypeMock.Object;

		Mock<IAccount> accountMock = new();

		accountMock.Setup(e => e.Id).Returns(accountId);
		accountMock.Setup(e => e.AccountTypeId).Returns(accountTypeId);

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var account = accountMock.Object;

		_accountTypeServiceMock
			.Setup(
				a =>
					a.GetAccountType(
						accountTypeId,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync((IAccountType?)accountType);

		decimal transactionFee = TransactionBusiness.GetTransactionFee(
			accountType,
			transactionType,
			amount);
		DateTime projectedRepaymentDate = TransactionBusiness.GetProjectedRepaymentDate(
			accountType,
			transactionDate);
		Identity<int> expected =
			new()
			{
				Value = expectedId
			};

		_accountServiceMock
			.Setup(
				a =>
					a.GetAccount(
						accountId,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync(account);

		Mock<IUser> userMock = new();

		userMock.Setup(e => e.Id).Returns(userId);

		// ReSharper disable once SuggestVarOrType_SimpleTypes
		var user = userMock.Object;

		_userServiceMock
			.Setup(
				a =>
					a.GetSingleUser(
						userId,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync(user);

		_transactionServiceMock
			.Setup(
				ts =>
					ts.CreateTransaction(
						userId,
						accountId,
						transactionType,
						amount,
						description,
						transactionDate,
						transactionFee,
						projectedRepaymentDate,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		_transactionServiceMock
			.Setup(
				ts =>
					ts.PublishNewTransactionToBalance(
						expected.Value,
						It.IsAny<CancellationToken>()));

		// Act
		Identity<int>? actual =
			await _transactionBusiness
				.CreateTransaction(
					userId,
					accountId,
					transactionType,
					amount,
					description,
					transactionDate,
					CancellationToken.None);

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.That(actual, Is.EqualTo(expected));
				_accountTypeServiceMock
					.Verify(
						a =>
							a.GetAccountType(
								accountTypeId,
								It.IsAny<CancellationToken>()),
						Times.Once);
				_userServiceMock
					.Verify(
						a =>
							a.GetSingleUser(
								userId,
								It.IsAny<CancellationToken>()),
						Times.Once);
				_transactionServiceMock
					.Verify(
						ts =>
							ts.CreateTransaction(
								userId,
								accountId,
								transactionType,
								amount,
								description,
								transactionDate,
								transactionFee,
								projectedRepaymentDate,
								It.IsAny<CancellationToken>()),
						Times.Once);
				_transactionServiceMock
					.Verify(
						ts =>
							ts.PublishNewTransactionToBalance(
								expected!.Value,
								It.IsAny<CancellationToken>()),
						Times.Once);
			});
	}

	#endregion
}
