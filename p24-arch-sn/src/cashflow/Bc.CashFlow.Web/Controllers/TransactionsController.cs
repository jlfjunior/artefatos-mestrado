using Bc.CashFlow.Domain.Transaction;
using Bc.CashFlow.Domain.User;
using Bc.CashFlow.Web.Models.Transaction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bc.CashFlow.Web.Controllers;

public class TransactionsController : Controller
{
	private readonly ITransactionBusiness _business;

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<TransactionsController> _logger;

	public TransactionsController(
		ILogger<TransactionsController> logger,
		ITransactionBusiness business)
	{
		_logger = logger;
		_business = business;
	}

	[HttpGet]
	public async Task<IActionResult> Index(
		[FromQuery(Name = "user-id")] int? userId,
		[FromQuery(Name = "account-id")] int? accountId,
		[FromQuery(Name = "transaction-type")] TransactionType? transactionType,
		[FromQuery(Name = "amount-from")] decimal? amountFrom,
		[FromQuery(Name = "amount-to")] decimal? amountTo,
		[FromQuery(Name = "transaction-date-since")]
		DateTime? transactionDateSince,
		[FromQuery(Name = "transaction-date-until")]
		DateTime? transactionDateUntil,
		[FromQuery(Name = "projected-repayment-date-since")]
		DateTime? projectedRepaymentDateSince,
		[FromQuery(Name = "projected-repayment-date-until")]
		DateTime? projectedRepaymentDateUntil,
		[FromQuery(Name = "paging-skip")] int? pagingSkip,
		[FromQuery(Name = "paging-limit")] int? pagingLimit,
		CancellationToken cancellationToken)
	{
		IEnumerable<ITransaction> transactionsList = await _business.GetTransactions(
			userId,
			accountId,
			transactionType,
			amountFrom,
			amountTo,
			transactionDateSince,
			transactionDateUntil,
			projectedRepaymentDateSince,
			projectedRepaymentDateUntil,
			pagingSkip ?? 0,
			pagingLimit ?? 20,
			cancellationToken);
		TransactionIndexViewModel viewModel = new(transactionsList.Take(100));

		return View(viewModel);
	}

	[HttpGet]
	public async Task<IActionResult> Details(
		[FromRoute] int id,
		CancellationToken cancellationToken)
	{
		ITransaction transaction = await _business.GetRequiredTransaction(
			id,
			cancellationToken);
		IUser? user = null;

		if (transaction.UserId is not null)
		{
			user = await _business.GetRequiredUser(
				transaction.UserId.Value,
				cancellationToken);
		}

		TransactionDetailsViewModel viewModel = new(
			transaction,
			user);

		return View(viewModel);
	}

	[HttpGet]
	public async Task<IActionResult> Create(
		[FromQuery(Name = "paging-skip")] int? pagingSkip,
		[FromQuery(Name = "paging-limit")] int? pagingLimit,
		CancellationToken cancellationToken)
	{
		TransactionCreateViewModel viewModel = new()
		{
			AccountsList = await GetAccountsSelectList(
				pagingSkip,
				pagingLimit,
				cancellationToken)
		};

		return View(viewModel);
	}

	[HttpPost]
	public async Task<IActionResult> Create(
		[FromForm(Name = "account-id")] int accountId,
		[FromForm(Name = "transaction-type")] int transactionType,
		[FromForm(Name = "amount")] decimal amount,
		[FromForm(Name = "description")] string? description,
		[FromQuery(Name = "paging-skip")] int? pagingSkip,
		[FromQuery(Name = "paging-limit")] int? pagingLimit,
		CancellationToken cancellationToken)
	{
		if (!Enum.IsDefined(typeof(TransactionType), transactionType)) throw new TransactionTypeOutOfRangeException();

		if (ModelState.IsValid)
		{
			_ = await _business.CreateTransaction(
				null,
				accountId,
				(TransactionType)transactionType,
				amount,
				description,
				DateTime.Now,
				cancellationToken);

			return RedirectToAction(nameof(Index));
		}

		TransactionCreateViewModel viewModel = new()
		{
			AccountsList = await GetAccountsSelectList(
				pagingSkip,
				pagingLimit,
				cancellationToken)
		};

		return View(viewModel);
	}

	private async Task<IEnumerable<SelectListItem>> GetAccountsSelectList(
		int? pagingSkip,
		int? pagingLimit,
		CancellationToken cancellationToken)
	{
		return (await _business.GetAccounts(
				pagingSkip,
				pagingLimit,
				cancellationToken))
			.Select(
				account => new SelectListItem(
					account.Name,
					account.Id.ToString())
			);
	}
}
