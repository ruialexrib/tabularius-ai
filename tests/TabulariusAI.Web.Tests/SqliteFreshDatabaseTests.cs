using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Data.Entities;
using Xunit;

namespace TabulariusAI.Web.Tests;

public sealed class SqliteFreshDatabaseTests
{
    [Fact]
    public async Task FreshSqliteDatabase_GeneratesIdentityKeysForCoreEntities()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<TabulariusDbContext>()
            .UseSqlite(connection)
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        await using var db = new TabulariusDbContext(options);
        await db.Database.MigrateAsync();

        var entity = new AccountingEntity
        {
            Name = "Test Entity",
            TaxRegistrationNumber = "999999990",
            CreatedAtUtc = DateTime.UtcNow
        };
        db.AccountingEntities.Add(entity);
        await db.SaveChangesAsync();
        Assert.True(entity.Id > 0);

        var dossier = new AnalysisDossier
        {
            AccountingEntityId = entity.Id,
            Name = "Test Dossier",
            FiscalYear = 2026,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.AnalysisDossiers.Add(dossier);
        await db.SaveChangesAsync();
        Assert.True(dossier.Id > 0);

        var import = new SaftImport
        {
            DossierId = dossier.Id,
            OriginalFileName = "test.xml",
            SaftVersion = "1.04_01",
            ImportedAtUtc = DateTime.UtcNow
        };
        db.SaftImports.Add(import);
        await db.SaveChangesAsync();
        Assert.True(import.Id > 0);
    }
}
