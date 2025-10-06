using System.Diagnostics;
using Bc.CashFlow.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bc.CashFlow.Web.Controllers;

public class HomeController : Controller
{
	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<HomeController> _logger;

	public HomeController(
		ILogger<HomeController> logger)
	{
		_logger = logger;
	}

	public IActionResult Index()
	{
		return View();
	}

	[ResponseCache(
		Duration = 0,
		Location = ResponseCacheLocation.None,
		NoStore = true)]
	public IActionResult Error()
	{
		return View(
			new ErrorViewModel
			{
				RequestId = Activity.Current?.Id
				            ?? HttpContext.TraceIdentifier
			});
	}
}