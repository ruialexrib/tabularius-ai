using Microsoft.AspNetCore.Mvc;
using TabulariusAI.Web.Services;

namespace TabulariusAI.Web.Controllers;

/// <summary>
/// Handles requests for the main Tabularius AI application pages.
/// </summary>
public sealed class HomeController : Controller
{
    private const long MaximumSaftFileSize = 100 * 1024 * 1024;
    private readonly ISaftHeaderReader _saftHeaderReader;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeController"/> class.
    /// </summary>
    /// <param name="saftHeaderReader">The service used to validate and read SAF-T header information.</param>
    public HomeController(ISaftHeaderReader saftHeaderReader)
    {
        _saftHeaderReader = saftHeaderReader;
    }

    /// <summary>
    /// Displays the application home page.
    /// </summary>
    /// <returns>The home page view.</returns>
    public IActionResult Index() => View();

    /// <summary>
    /// Validates an uploaded SAF-T XML file and displays its header information.
    /// </summary>
    /// <param name="saftFile">The SAF-T XML file uploaded by the user.</param>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    /// <returns>The home page containing either the extracted header or a validation error.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = MaximumSaftFileSize)]
    [RequestSizeLimit(MaximumSaftFileSize)]
    public async Task<IActionResult> UploadSaftAsync(IFormFile? saftFile, CancellationToken cancellationToken)
    {
        if (saftFile is null || saftFile.Length == 0)
        {
            ModelState.AddModelError("saftFile", "Selecione um ficheiro SAF-T em formato XML.");
            return View("Index");
        }

        if (saftFile.Length > MaximumSaftFileSize)
        {
            ModelState.AddModelError("saftFile", "O ficheiro SAF-T não pode exceder 100 MB.");
            return View("Index");
        }

        if (!string.Equals(Path.GetExtension(saftFile.FileName), ".xml", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("saftFile", "O ficheiro selecionado deve ter a extensão .xml.");
            return View("Index");
        }

        try
        {
            await using var stream = saftFile.OpenReadStream();
            var header = await _saftHeaderReader.ReadAsync(stream, cancellationToken);
            return View("Index", header);
        }
        catch (InvalidDataException exception)
        {
            ModelState.AddModelError("saftFile", exception.Message);
            return View("Index");
        }
    }

    /// <summary>
    /// Displays the generic application error page.
    /// </summary>
    /// <returns>The error page view.</returns>
    public IActionResult Error() => View();
}
