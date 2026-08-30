using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
        var options = new DbContextOptionsBuilder<TabulariusDbContext>().UseSqlite(connection).Options;
        await using var db = new TabulariusDbContext(options);
        await db.Database.MigrateAsync();

        var entity = new AccountingEntity { Name = "Test Entity", TaxRegistrationNumber = "999999990", CreatedAtUtc = DateTime.UtcNow };
        db.AccountingEntities.Add(entity);
        await db.SaveChangesAsync();

        Assert.True(entity.Id > 0);
    }
}
