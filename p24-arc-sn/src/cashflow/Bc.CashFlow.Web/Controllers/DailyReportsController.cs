using Bc.CashFlow.Domain.DailyReport;
using Bc.CashFlow.Web.Models.DailyReport;
using Microsoft.AspNetCore.Mvc;

namespace Bc.CashFlow.Web.Controllers;

public class DailyReportsController : Controller
{
	private readonly IDailyReportBusiness _business;

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<DailyReportsController> _logger;

	public DailyReportsController(
		ILogger<DailyReportsController> logger,
		IDailyReportBusiness business)
	{
		_logger = logger;
		_business = business;
	}

	[HttpGet]
	public async Task<IActionResult> Index(
		[FromQuery(Name = "reference-date-since")]
		DateTime? referenceDateSince,
		[FromQuery(Name = "reference-date-until")]
		DateTime? referenceDateUntil,
		CancellationToken cancellationToken)
	{
		IEnumerable<IDailyReport> dailyReportsList =
			await _business.GetDailyReports(
				referenceDateSince,
				referenceDateUntil,
				cancellationToken);
		DailyReportIndexViewModel viewModel = new(dailyReportsList);

		return View(viewModel);
	}

	[HttpGet]
	public IActionResult Create()
	{
		return View();
	}

	[HttpPost]
	public async Task<IActionResult> Create(
		DailyReportCreateViewModel viewModel,
		[FromForm(Name = "reference-date")] DateTime referenceDate,
		CancellationToken cancellationToken)
	{
		if (!ModelState.IsValid)
		{
			return View(viewModel);
		}

		await _business.ConsolidateDailyReport(
			referenceDate,
			cancellationToken);

		return RedirectToAction(nameof(Index));
	}
}
