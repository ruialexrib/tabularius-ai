using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Models;
using TabulariusAI.Web.Services;

namespace TabulariusAI.Web.Controllers;

public sealed partial class DossierController
{
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DownloadBackup(int id, [FromServices] IDossierBackupService backupService, [FromServices] ApplicationInfo appInfo, CancellationToken cancellationToken)
    {
        try
        {
            var dossier = await dbContext.AnalysisDossiers.AsNoTracking().Include(x => x.AccountingEntity).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (dossier is null) return NotFound();
            var data = await backupService.ExportAsync(id, appInfo.Version, cancellationToken);
            var safeName = string.Concat(dossier.AccountingEntity.Name.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')).Trim('-');
            return File(data, "application/json", $"tabularius-{safeName}-{dossier.FiscalYear}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpGet]
    public IActionResult RestoreBackup() => View(new DossierRestoreViewModel());

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(104_857_600)]
    public async Task<IActionResult> RestoreBackup(DossierRestoreViewModel model, [FromServices] IDossierBackupService backupService, CancellationToken cancellationToken)
    {
        if (model.BackupFile is null || model.BackupFile.Length == 0) ModelState.AddModelError(nameof(model.BackupFile), "Selecione um ficheiro de backup JSON.");
        else if (model.BackupFile.Length > 104_857_600) ModelState.AddModelError(nameof(model.BackupFile), "O ficheiro não pode exceder 100 MB.");
        else if (!string.Equals(Path.GetExtension(model.BackupFile.FileName), ".json", StringComparison.OrdinalIgnoreCase)) ModelState.AddModelError(nameof(model.BackupFile), "Selecione um ficheiro JSON.");
        if (!ModelState.IsValid) return View(model);
        try
        {
            await using var stream = model.BackupFile!.OpenReadStream();
            var result = await backupService.RestoreAsync(stream, cancellationToken);
            TempData["SuccessMessage"] = $"Dossier restaurado com sucesso: {result.Imports} importações e {result.Records} registos.";
            return RedirectToAction(nameof(Details), new { id = result.DossierId });
        }
        catch (InvalidDataException exception) { ModelState.AddModelError(nameof(model.BackupFile), exception.Message); return View(model); }
        catch (DbUpdateException) { ModelState.AddModelError(string.Empty, "O backup contém dados incompatíveis ou relações inválidas. Nenhuma alteração foi aplicada."); return View(model); }
    }
}
