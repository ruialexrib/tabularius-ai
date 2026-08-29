using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data.Entities;
using TabulariusAI.Web.Tests.Infrastructure;
using Xunit;

namespace TabulariusAI.Web.Tests.Data;

/// <summary>Verifies relational constraints and cascade behavior in the application persistence model.</summary>
public sealed class TabulariusDbContextTests
{
    /// <summary>Verifies that Portuguese tax registration numbers uniquely identify accounting entities.</summary>
    [Fact]
    public async Task AccountingEntities_DuplicateTaxNumber_IsRejected()
    {
        await using var database = new TestDatabase();
        database.Context.AccountingEntities.Add(new AccountingEntity { Name = "First", TaxRegistrationNumber = "500000100" });
        await database.Context.SaveChangesAsync();
        database.Context.AccountingEntities.Add(new AccountingEntity { Name = "Second", TaxRegistrationNumber = "500000100" });

        await Assert.ThrowsAsync<DbUpdateException>(() => database.Context.SaveChangesAsync());
    }

    /// <summary>Verifies that one entity cannot contain two dossiers for the same fiscal year.</summary>
    [Fact]
    public async Task AnalysisDossiers_DuplicateFiscalYearForEntity_IsRejected()
    {
        await using var database = new TestDatabase();
        var entity = new AccountingEntity { Name = "Entity", TaxRegistrationNumber = "500000101" };
        database.Context.AccountingEntities.Add(entity);
        await database.Context.SaveChangesAsync();
        database.Context.AnalysisDossiers.Add(new AnalysisDossier { AccountingEntityId = entity.Id, Name = "First", FiscalYear = 2026 });
        await database.Context.SaveChangesAsync();
        database.Context.AnalysisDossiers.Add(new AnalysisDossier { AccountingEntityId = entity.Id, Name = "Second", FiscalYear = 2026 });

        await Assert.ThrowsAsync<DbUpdateException>(() => database.Context.SaveChangesAsync());
    }

    /// <summary>Verifies that deleting an accounting entity cascades through dossiers and SAF-T (PT) imports.</summary>
    [Fact]
    public async Task AccountingEntity_Delete_CascadesToDossiersAndImports()
    {
        await using var database = new TestDatabase();
        var entity = new AccountingEntity { Name = "Entity", TaxRegistrationNumber = "500000102" };
        database.Context.AccountingEntities.Add(entity);
        await database.Context.SaveChangesAsync();
        var dossier = new AnalysisDossier { AccountingEntityId = entity.Id, Name = "2026", FiscalYear = 2026 };
        database.Context.AnalysisDossiers.Add(dossier);
        await database.Context.SaveChangesAsync();
        database.Context.SaftImports.Add(new SaftImport { DossierId = dossier.Id, OriginalFileName = "source.xml", SaftVersion = "1.04_01" });
        await database.Context.SaveChangesAsync();

        database.Context.AccountingEntities.Remove(entity);
        await database.Context.SaveChangesAsync();

        Assert.Empty(await database.Context.AnalysisDossiers.AsNoTracking().ToListAsync());
        Assert.Empty(await database.Context.SaftImports.AsNoTracking().ToListAsync());
    }

    /// <summary>Verifies that the same SAF-T account identifier cannot occur twice inside one import.</summary>
    [Fact]
    public async Task SaftAccounts_DuplicateAccountWithinImport_IsRejected()
    {
        await using var database = new TestDatabase();
        var import = await SeedImportAsync(database);
        database.Context.SaftAccounts.Add(CreateAccount(import.Id, "11", "Caixa"));
        await database.Context.SaveChangesAsync();
        database.Context.SaftAccounts.Add(CreateAccount(import.Id, "11", "Caixa duplicada"));

        await Assert.ThrowsAsync<DbUpdateException>(() => database.Context.SaveChangesAsync());
    }

    /// <summary>Creates a minimal persisted dossier and SAF-T (PT) import for relational tests.</summary>
    private static async Task<SaftImport> SeedImportAsync(TestDatabase database)
    {
        var entity = new AccountingEntity { Name = "Entity", TaxRegistrationNumber = Guid.NewGuid().ToString("N")[..9] };
        database.Context.AccountingEntities.Add(entity);
        await database.Context.SaveChangesAsync();
        var dossier = new AnalysisDossier { AccountingEntityId = entity.Id, Name = "2026", FiscalYear = 2026 };
        database.Context.AnalysisDossiers.Add(dossier);
        await database.Context.SaveChangesAsync();
        var import = new SaftImport { DossierId = dossier.Id, OriginalFileName = "source.xml", SaftVersion = "1.04_01" };
        database.Context.SaftImports.Add(import);
        await database.Context.SaveChangesAsync();
        return import;
    }

    /// <summary>Creates a minimal SAF-T (PT) account row for persistence tests.</summary>
    private static SaftAccount CreateAccount(int importId, string accountId, string description) => new()
    {
        SaftImportId = importId,
        AccountId = accountId,
        Description = description
    };
}
