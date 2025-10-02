using Microsoft.AspNetCore.Mvc;

namespace EasyCash.Api.Controllers;

public sealed class HomeController : ControllerBase
{
    public IActionResult Index()
    {
        return Redirect("~/swagger");
    }
}
