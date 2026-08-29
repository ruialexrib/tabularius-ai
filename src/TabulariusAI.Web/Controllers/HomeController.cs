using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Data.Entities;
using TabulariusAI.Web.Services;

namespace TabulariusAI.Web.Controllers;

/// <summary>
/// Handles requests for the main Tabularius AI application pages.
/// </summary>
public sealed class HomeController : Controller
{
    private const long MaximumSaftFileSize = 100 * 1024 * 1024;
    private readonly ISaftHeaderReader _saftHeaderReader;
    private readonly TabulariusDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeController"/> class.
    /// </summary>
    /// <param name="saftHeaderReader">The service used to validate and read SAF-T (PT) information.</param>
    /// <param name="dbContext">The local application persistence context.</param>
    public HomeController(ISaftHeaderReader saftHeaderReader, TabulariusDbContext dbContext)
    {
        _saftHeaderReader = saftHeaderReader;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Displays the application home page.
    /// </summary>
    /// <returns>The home page view.</returns>
    public IActionResult Index() => View();

    /// <summary>
    /// Validates an uploaded SAF-T (PT) XML file, persists its dossier metadata and displays the analysis.
    /// </summary>
    /// <param name="saftFile">The SAF-T (PT) XML file uploaded by the user.</param>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    /// <returns>The home page containing either the extracted analysis or a validation error.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = MaximumSaftFileSize)]
    [RequestSizeLimit(MaximumSaftFileSize)]
    public async Task<IActionResult> UploadSaftAsync(IFormFile? saftFile, CancellationToken cancellationToken)
    {
        if (saftFile is null || saftFile.Length == 0)
        {
            ModelState.AddModelError("saftFile", "Selecione um ficheiro SAF-T (PT) em formato XML.");
            return View("Index");
        }

        if (saftFile.Length > MaximumSaftFileSize)
        {
            ModelState.AddModelError("saftFile", "O ficheiro SAF-T (PT) não pode exceder 100 MB.");
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
            var analysis = await _saftHeaderReader.ReadAsync(stream, cancellationToken);
            await PersistImportAsync(saftFile.FileName, analysis, cancellationToken);
            return View("Index", analysis);
        }
        catch (InvalidDataException exception)
        {
            ModelState.AddModelError("saftFile", exception.Message);
            return View("Index");
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("saftFile", "O ficheiro foi analisado, mas não foi possível guardar os dados do dossier localmente.");
            return View("Index");
        }
    }

    /// <summary>
    /// Displays the generic application error page.
    /// </summary>
    /// <returns>The error page view.</returns>
    public IActionResult Error() => View();

    /// <summary>
    /// Creates or reuses the accounting entity and fiscal dossier associated with an imported SAF-T (PT) file.
    /// </summary>
    /// <param name="fileName">The original uploaded file name.</param>
    /// <param name="analysis">The validated SAF-T (PT) analysis.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous persistence operation.</returns>
    private async Task PersistImportAsync(string fileName, Models.SaftHeaderViewModel analysis, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.AccountingEntities
            .SingleOrDefaultAsync(item => item.TaxRegistrationNumber == analysis.TaxRegistrationNumber, cancellationToken);

        if (entity is null)
        {
            entity = new AccountingEntity
            {
                Name = analysis.CompanyName,
                TaxRegistrationNumber = analysis.TaxRegistrationNumber
            };
            _dbContext.AccountingEntities.Add(entity);
        }
        else if (!string.Equals(entity.Name, analysis.CompanyName, StringComparison.Ordinal))
        {
            entity.Name = analysis.CompanyName;
        }

        var fiscalYear = int.TryParse(analysis.FiscalYear, NumberStyles.None, CultureInfo.InvariantCulture, out var parsedYear)
            ? parsedYear
            : 0;

        var dossier = await _dbContext.AnalysisDossiers
            .SingleOrDefaultAsync(item => item.AccountingEntityId == entity.Id && item.FiscalYear == fiscalYear, cancellationToken);

        if (dossier is null)
        {
            dossier = new AnalysisDossier
            {
                AccountingEntity = entity,
                FiscalYear = fiscalYear,
                Name = fiscalYear > 0 ? $"Exercício {fiscalYear}" : "Período contabilístico"
            };
            _dbContext.AnalysisDossiers.Add(dossier);
        }

        dossier.Imports.Add(new SaftImport
        {
            OriginalFileName = Path.GetFileName(fileName),
            SaftVersion = analysis.SaftVersion,
            StartDate = ParseDate(analysis.StartDate),
            EndDate = ParseDate(analysis.EndDate)
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Parses a SAF-T ISO date value into a date-only representation.
    /// </summary>
    /// <param name="value">The SAF-T date value.</param>
    /// <returns>The parsed date, or <see langword="null"/> when the value is invalid.</returns>
    private static DateOnly? ParseDate(string value) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;
}
