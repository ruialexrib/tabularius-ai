using Microsoft.AspNetCore.Mvc;
using TabulariusAI.Web.Controllers;
using TabulariusAI.Web.Data.Entities;
using TabulariusAI.Web.Models;
using TabulariusAI.Web.Tests.Infrastructure;
using Xunit;

namespace TabulariusAI.Web.Tests.Controllers;

/// <summary>Verifies dossier navigation, source selection, filtering and paging behavior.</summary>
public sealed class DossierControllerTests
{
    /// <summary>Verifies that the latest accounting period is selected even when an older period was imported later.</summary>
    [Fact]
    public async Task SaftSummary_WithoutImportId_SelectsLatestAccountingPeriod()
    {
        await using var database = new TestDatabase();
        var dossier = await SeedDossierAsync(database);
        var olderPeriod = CreateImport(dossier.Id, "older.xml", new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 31), new DateTime(2026, 8, 20, 12, 0, 0, DateTimeKind.Utc));
        var newerPeriod = CreateImport(dossier.Id, "newer.xml", new DateOnly(2026, 4, 1), new DateOnly(2026, 6, 30), new DateTime(2026, 8, 10, 12, 0, 0, DateTimeKind.Utc));
        database.Context.SaftImports.AddRange(olderPeriod, newerPeriod);
        await database.Context.SaveChangesAsync();

        var result = await new DossierController(database.Context).SaftSummary(dossier.Id, null, CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SaftImportSelectionViewModel>(view.Model);
        Assert.Equal(newerPeriod.Id, model.SelectedImport.Id);
    }

    /// <summary>Verifies that equal accounting periods use import time and then identifier as deterministic tie breakers.</summary>
    [Fact]
    public async Task SaftSummary_SamePeriod_SelectsLatestImportThenHighestId()
    {
        await using var database = new TestDatabase();
        var dossier = await SeedDossierAsync(database);
        var timestamp = new DateTime(2026, 8, 20, 12, 0, 0, DateTimeKind.Utc);
        var first = CreateImport(dossier.Id, "first.xml", new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 31), timestamp);
        var second = CreateImport(dossier.Id, "second.xml", new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 31), timestamp);
        database.Context.SaftImports.AddRange(first, second);
        await database.Context.SaveChangesAsync();

        var result = await new DossierController(database.Context).SaftSummary(dossier.Id, null, CancellationToken.None);

        var model = Assert.IsType<SaftImportSelectionViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(second.Id, model.SelectedImport.Id);
    }

    /// <summary>Verifies that an explicitly requested import is preserved instead of silently selecting another source.</summary>
    [Fact]
    public async Task SaftSummary_WithImportId_SelectsRequestedSource()
    {
        await using var database = new TestDatabase();
        var dossier = await SeedDossierAsync(database);
        var first = CreateImport(dossier.Id, "first.xml", new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 31), DateTime.UtcNow.AddDays(-1));
        var second = CreateImport(dossier.Id, "second.xml", new DateOnly(2026, 4, 1), new DateOnly(2026, 6, 30), DateTime.UtcNow);
        database.Context.SaftImports.AddRange(first, second);
        await database.Context.SaveChangesAsync();

        var result = await new DossierController(database.Context).SaftSummary(dossier.Id, first.Id, CancellationToken.None);

        var model = Assert.IsType<SaftImportSelectionViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(first.Id, model.SelectedImport.Id);
    }

    /// <summary>Verifies that an import belonging to another dossier cannot be selected through the current dossier route.</summary>
    [Fact]
    public async Task SaftSummary_WithImportFromAnotherDossier_ReturnsNotFound()
    {
        await using var database = new TestDatabase();
        var firstDossier = await SeedDossierAsync(database, "Entity A", "500000001", 2026);
        var secondDossier = await SeedDossierAsync(database, "Entity B", "500000002", 2026);
        var firstImport = CreateImport(firstDossier.Id, "a.xml", new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 31), DateTime.UtcNow);
        var secondImport = CreateImport(secondDossier.Id, "b.xml", new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 31), DateTime.UtcNow);
        database.Context.SaftImports.AddRange(firstImport, secondImport);
        await database.Context.SaveChangesAsync();

        var result = await new DossierController(database.Context).SaftSummary(firstDossier.Id, secondImport.Id, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>Verifies entity filtering and normalization of unsupported page sizes.</summary>
    [Fact]
    public async Task Entities_SearchAndInvalidPageSize_ReturnsFilteredDefaultPage()
    {
        await using var database = new TestDatabase();
        database.Context.AccountingEntities.AddRange(
            new AccountingEntity { Name = "Alpha Consulting", TaxRegistrationNumber = "500000010" },
            new AccountingEntity { Name = "Beta Services", TaxRegistrationNumber = "500000011" });
        await database.Context.SaveChangesAsync();

        var result = await new DossierController(database.Context).Entities(" Alpha ", 0, 999, CancellationToken.None);

        var model = Assert.IsType<PagedListViewModel<AccountingEntity>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Single(model.Items);
        Assert.Equal("Alpha Consulting", model.Items[0].Name);
        Assert.Equal(1, model.Page);
        Assert.Equal(10, model.PageSize);
        Assert.Equal("Alpha", model.Search);
    }

    /// <summary>Verifies that requesting a missing entity returns HTTP not found semantics.</summary>
    [Fact]
    public async Task Entity_MissingEntity_ReturnsNotFound()
    {
        await using var database = new TestDatabase();

        var result = await new DossierController(database.Context).Entity(999, null, cancellationToken: CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>Creates and persists an accounting entity with one analysis dossier.</summary>
    private static async Task<AnalysisDossier> SeedDossierAsync(TestDatabase database, string entityName = "Test Entity", string taxId = "500000000", int fiscalYear = 2026)
    {
        var entity = new AccountingEntity { Name = entityName, TaxRegistrationNumber = taxId };
        database.Context.AccountingEntities.Add(entity);
        await database.Context.SaveChangesAsync();
        var dossier = new AnalysisDossier { AccountingEntityId = entity.Id, Name = $"Exercício {fiscalYear}", FiscalYear = fiscalYear };
        database.Context.AnalysisDossiers.Add(dossier);
        await database.Context.SaveChangesAsync();
        return dossier;
    }

    /// <summary>Creates a SAF-T (PT) import with deterministic source metadata for a dossier.</summary>
    private static SaftImport CreateImport(int dossierId, string fileName, DateOnly startDate, DateOnly endDate, DateTime importedAtUtc) => new()
    {
        DossierId = dossierId,
        OriginalFileName = fileName,
        SaftVersion = "1.04_01",
        StartDate = startDate,
        EndDate = endDate,
        ImportedAtUtc = importedAtUtc
    };
}
