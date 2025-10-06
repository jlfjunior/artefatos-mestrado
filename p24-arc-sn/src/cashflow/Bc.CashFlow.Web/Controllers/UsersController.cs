using Bc.CashFlow.Domain.User;
using Bc.CashFlow.Web.Models.User;
using Microsoft.AspNetCore.Mvc;

namespace Bc.CashFlow.Web.Controllers;

public class UsersController : Controller
{
	private readonly IUserBusiness _business;

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<UsersController> _logger;

	public UsersController(
		ILogger<UsersController> logger,
		IUserBusiness business)
	{
		_logger = logger;
		_business = business;
	}

	[HttpGet]
	public async Task<IActionResult> Index(
		[FromQuery(Name = "username")] string? username,
		[FromQuery(Name = "created-since")] DateTime? createdSince,
		[FromQuery(Name = "created-until")] DateTime? createdUntil,
		CancellationToken cancellationToken)
	{
		IEnumerable<IUser> usersList =
			await _business.GetUsers(
				username,
				createdSince,
				createdUntil,
				cancellationToken);
		UserIndexViewModel viewModel = new(usersList);

		return View(viewModel);
	}

	[HttpGet]
	public async Task<IActionResult> Details(
		[FromRoute(Name = "id")] int userId,
		CancellationToken cancellationToken)
	{
		IUser user =
			await _business.GetRequiredUser(
				userId,
				cancellationToken);
		UserDetailsViewModel viewModel = new(user);

		return View(viewModel);
	}
}