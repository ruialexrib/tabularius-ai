using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
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
public sealed class HomeController(ISaftHeaderReader saftHeaderReader, TabulariusDbContext dbContext) : Controller
{
    private const long MaximumSaftFileSize = 100 * 1024 * 1024;

    /// <summary>Displays the application presentation page.</summary>
    /// <returns>The home page view.</returns>
    public IActionResult Index() => View();

    /// <summary>Displays the SAF-T (PT) import workspace.</summary>
    /// <returns>The import view.</returns>
    [HttpGet]
    public IActionResult Import() => View();

    /// <summary>Processes an uploaded SAF-T (PT) XML file and persists its dossier data.</summary>
    /// <param name="saftFile">The SAF-T (PT) XML file uploaded by the user.</param>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    /// <returns>The import page containing either the extracted result or an import error.</returns>
    [HttpPost, ValidateAntiForgeryToken, RequestFormLimits(MultipartBodyLengthLimit = MaximumSaftFileSize), RequestSizeLimit(MaximumSaftFileSize)]
    public async Task<IActionResult> UploadSaftAsync(IFormFile? saftFile, CancellationToken cancellationToken)
    {
        if (saftFile is null || saftFile.Length == 0) { ModelState.AddModelError("saftFile", "Selecione um ficheiro SAF-T (PT) em formato XML."); return View("Import"); }
        if (saftFile.Length > MaximumSaftFileSize) { ModelState.AddModelError("saftFile", "O ficheiro SAF-T (PT) não pode exceder 100 MB."); return View("Import"); }
        if (!string.Equals(Path.GetExtension(saftFile.FileName), ".xml", StringComparison.OrdinalIgnoreCase)) { ModelState.AddModelError("saftFile", "O ficheiro selecionado deve ter a extensão .xml."); return View("Import"); }
        try
        {
            var contentHash = await CalculateContentHashAsync(saftFile, cancellationToken);
            if (await dbContext.SaftImports.AsNoTracking().AnyAsync(item => item.ContentHash == contentHash, cancellationToken))
            {
                ModelState.AddModelError("saftFile", "Este ficheiro SAF-T (PT) já foi importado. Selecione um ficheiro diferente.");
                return View("Import");
            }

            await using var stream = saftFile.OpenReadStream();
            var analysis = await saftHeaderReader.ReadAsync(stream, cancellationToken);
            await PersistImportAsync(saftFile.FileName, contentHash, analysis, cancellationToken);
            TempData["SaftImportValidation"] = "Ficheiro SAF-T (PT) analisado e importado com sucesso.";
            return View("Import", analysis);
        }
        catch (InvalidDataException exception) { ModelState.AddModelError("saftFile", exception.Message); return View("Import"); }
        catch (DbUpdateException) { ModelState.AddModelError("saftFile", "O ficheiro foi analisado, mas não foi possível guardar os dados do dossier localmente."); return View("Import"); }
    }

    /// <summary>Displays the generic application error page with a request identifier for log correlation.</summary>
    /// <returns>The error page view.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    /// <summary>Calculates a stable SHA-256 identity for the exact uploaded SAF-T (PT) content.</summary>
    /// <param name="saftFile">The uploaded SAF-T (PT) file.</param><param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The lowercase hexadecimal SHA-256 hash.</returns>
    private static async Task<string> CalculateContentHashAsync(IFormFile saftFile, CancellationToken cancellationToken)
    {
        await using var stream = saftFile.OpenReadStream();
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexStringLower(hash);
    }

    /// <summary>Creates or reuses the entity and dossier and persists source-traceable SAF-T (PT) data.</summary>
    /// <param name="fileName">The original uploaded file name.</param><param name="contentHash">The SHA-256 identity of the exact source file.</param><param name="analysis">The parsed SAF-T (PT) analysis.</param><param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous persistence operation.</returns>
    private async Task PersistImportAsync(string fileName, string contentHash, SaftHeaderViewModel analysis, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var entity = await dbContext.AccountingEntities.SingleOrDefaultAsync(item => item.TaxRegistrationNumber == analysis.TaxRegistrationNumber, cancellationToken);
        if (entity is null) { entity = new AccountingEntity { Name = analysis.CompanyName, TaxRegistrationNumber = analysis.TaxRegistrationNumber }; dbContext.AccountingEntities.Add(entity); await dbContext.SaveChangesAsync(cancellationToken); }
        else if (!string.Equals(entity.Name, analysis.CompanyName, StringComparison.Ordinal)) entity.Name = analysis.CompanyName;
        if (!int.TryParse(analysis.FiscalYear, NumberStyles.None, CultureInfo.InvariantCulture, out var fiscalYear) || fiscalYear <= 0) throw new InvalidDataException("O SAF-T (PT) não contém um exercício fiscal válido.");
        var dossier = await dbContext.AnalysisDossiers.SingleOrDefaultAsync(item => item.AccountingEntityId == entity.Id && item.FiscalYear == fiscalYear, cancellationToken);
        if (dossier is null) { dossier = new AnalysisDossier { AccountingEntityId = entity.Id, FiscalYear = fiscalYear, Name = $"Exercício {fiscalYear}" }; dbContext.AnalysisDossiers.Add(dossier); }
        var import = new SaftImport { OriginalFileName = Path.GetFileName(fileName), ContentHash = contentHash, SaftVersion = analysis.SaftVersion, StartDate = ParseDate(analysis.StartDate), EndDate = ParseDate(analysis.EndDate) };
        foreach (var source in analysis.Accounts) import.Accounts.Add(new SaftAccount { AccountId = source.AccountId, Description = source.Description, OpeningDebitBalance = source.OpeningDebitBalance, OpeningCreditBalance = source.OpeningCreditBalance, ClosingDebitBalance = source.ClosingDebitBalance, ClosingCreditBalance = source.ClosingCreditBalance, TaxonomyReference = source.TaxonomyReference });
        foreach (var source in analysis.Customers) import.Customers.Add(new SaftCustomer { CustomerId = source.PartyId, AccountId = source.AccountId, TaxId = source.TaxId, CompanyName = source.CompanyName });
        foreach (var source in analysis.Suppliers) import.Suppliers.Add(new SaftSupplier { SupplierId = source.PartyId, AccountId = source.AccountId, TaxId = source.TaxId, CompanyName = source.CompanyName });
        dossier.Imports.Add(import);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    /// <summary>Parses a SAF-T ISO date value into a date-only representation.</summary>
    /// <param name="value">The SAF-T date value.</param><returns>The parsed date, or <see langword="null"/> when the value is invalid.</returns>
    private static DateOnly? ParseDate(string value) => DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;
}
