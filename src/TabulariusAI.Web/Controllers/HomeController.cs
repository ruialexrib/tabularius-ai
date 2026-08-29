using Microsoft.AspNetCore.Mvc;

namespace TabulariusAI.Web.Controllers;

/// <summary>
/// Handles requests for the main Tabularius AI application pages.
/// </summary>
public sealed class HomeController : Controller
{
    /// <summary>
    /// Displays the application home page.
    /// </summary>
    /// <returns>The home page view.</returns>
    public IActionResult Index() => View();

    /// <summary>
    /// Displays the generic application error page.
    /// </summary>
    /// <returns>The error page view.</returns>
    public IActionResult Error() => View();
}
