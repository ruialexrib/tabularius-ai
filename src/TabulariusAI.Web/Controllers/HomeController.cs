using Microsoft.AspNetCore.Mvc;

namespace TabulariusAI.Web.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Error() => View();
}
