using Microsoft.AspNetCore.Mvc;

namespace EasyCash.Report.Api.Controllers;

public sealed class HomeController : ControllerBase
{
    public IActionResult Index()
    {
        return Redirect("~/swagger");
    }
}
