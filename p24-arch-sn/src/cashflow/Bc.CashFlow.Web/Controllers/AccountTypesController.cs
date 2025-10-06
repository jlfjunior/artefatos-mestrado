using Bc.CashFlow.Domain.AccountType;
using Bc.CashFlow.Web.Models.AccountType;
using Microsoft.AspNetCore.Mvc;

namespace Bc.CashFlow.Web.Controllers;

public class AccountTypesController : Controller
{
	private readonly IAccountTypeBusiness _business;

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<UsersController> _logger;

	public AccountTypesController(
		ILogger<UsersController> logger,
		IAccountTypeBusiness business)
	{
		_logger = logger;
		_business = business;
	}

	[HttpGet]
	public async Task<IActionResult> Index(
		string? name,
		decimal? baseFeeFrom,
		decimal? baseFeeTo,
		int? paymentDueDaysFrom,
		int? paymentDueDaysTo,
		[FromQuery(Name = "paging-skip")] int? pagingSkip,
		[FromQuery(Name = "paging-limit")] int? pagingLimit,
		CancellationToken cancellationToken)
	{
		IEnumerable<IAccountType> accountTypes =
			await _business.GetAccountTypes(
				name,
				baseFeeFrom,
				baseFeeTo,
				paymentDueDaysFrom,
				paymentDueDaysTo,
				pagingSkip,
				pagingLimit,
				cancellationToken);
		AccountTypeIndexViewModel viewModel = new(accountTypes);

		return View(viewModel);
	}
}
