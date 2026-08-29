using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Data.Entities;
using TabulariusAI.Web.Models;
using TabulariusAI.Web.Services;

namespace TabulariusAI.Web.Controllers;

/// <summary>
/// Handles requests for the main Tabularius AI application pages.
/// </summary>
public sealed class HomeController
    (ISaftHeaderReader saftHeaderReader, TabulariusDbContext dbContext) : Controller
{
    private const long MaximumSaftFileSize = 100 * 1024 * 1024;

    /// <summary>Displays the application home page.</summary>
    /// <returns>The home page view.</returns>
    public IActionResult Index() => View();

    /// <summary>Validates an uploaded SAF-T (PT) XML file, persists its dossier data and displays the analysis.</summary>
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
            var analysis = await saftHeaderReader.ReadAsync(stream, cancellationToken);
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

    /// <summary>Displays the generic application error page with a request identifier for log correlation.</summary>
    /// <returns>The error page view.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    /// <summary>Creates or reuses the entity and dossier and persists source-traceable SAF-T (PT) data.</summary>
    /// <param name="fileName">The original uploaded file name.</param>
    /// <param name="analysis">The validated SAF-T (PT) analysis.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous persistence operation.</returns>
    private async Task PersistImportAsync(string fileName, SaftHeaderViewModel analysis, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var entity = await dbContext.AccountingEntities.SingleOrDefaultAsync(item => item.TaxRegistrationNumber == analysis.TaxRegistrationNumber, cancellationToken);
        if (entity is null)
        {
            entity = new AccountingEntity { Name = analysis.CompanyName, TaxRegistrationNumber = analysis.TaxRegistrationNumber };
            dbContext.AccountingEntities.Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (!string.Equals(entity.Name, analysis.CompanyName, StringComparison.Ordinal))
        {
            entity.Name = analysis.CompanyName;
        }

        if (!int.TryParse(analysis.FiscalYear, NumberStyles.None, CultureInfo.InvariantCulture, out var fiscalYear) || fiscalYear <= 0)
        {
            throw new InvalidDataException("O SAF-T (PT) não contém um exercício fiscal válido.");
        }

        var dossier = await dbContext.AnalysisDossiers.SingleOrDefaultAsync(item => item.AccountingEntityId == entity.Id && item.FiscalYear == fiscalYear, cancellationToken);
        if (dossier is null)
        {
            dossier = new AnalysisDossier { AccountingEntityId = entity.Id, FiscalYear = fiscalYear, Name = $"Exercício {fiscalYear}" };
            dbContext.AnalysisDossiers.Add(dossier);
        }

        var import = new SaftImport
        {
            OriginalFileName = Path.GetFileName(fileName),
            SaftVersion = analysis.SaftVersion,
            StartDate = ParseDate(analysis.StartDate),
            EndDate = ParseDate(analysis.EndDate)
        };
        foreach (var source in analysis.Accounts)
        {
            import.Accounts.Add(new SaftAccount
            {
                AccountId = source.AccountId,
                Description = source.Description,
                OpeningDebitBalance = source.OpeningDebitBalance,
                OpeningCreditBalance = source.OpeningCreditBalance,
                ClosingDebitBalance = source.ClosingDebitBalance,
                ClosingCreditBalance = source.ClosingCreditBalance,
                TaxonomyReference = source.TaxonomyReference
            });
        }
        dossier.Imports.Add(import);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    /// <summary>Parses a SAF-T ISO date value into a date-only representation.</summary>
    /// <param name="value">The SAF-T date value.</param>
    /// <returns>The parsed date, or <see langword="null"/> when the value is invalid.</returns>
    private static DateOnly? ParseDate(string value) => DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;
}
