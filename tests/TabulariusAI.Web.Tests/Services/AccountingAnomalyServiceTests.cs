using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Data.Entities;
using TabulariusAI.Web.Services;
using Xunit;

namespace TabulariusAI.Web.Tests.Services;

public sealed class AccountingAnomalyServiceTests
{
    [Fact]
    public async Task EvaluateAsync_ReturnsNoFindings_ForBalancedValidTransaction()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var transaction = fixture.AddTransaction("TX-1", new DateOnly(2026, 6, 15));
        fixture.AddLine(transaction, "1", "1111", "D", 100m);
        fixture.AddLine(transaction, "2", "1211", "C", 100m);
        await fixture.Db.SaveChangesAsync();

        var findings = await new AccountingAnomalyService(fixture.Db).EvaluateAsync(fixture.Import.Id, fixture.Import.StartDate, fixture.Import.EndDate);

        Assert.Empty(findings);
    }

    [Fact]
    public async Task EvaluateAsync_DetectsAllFiveAccountingRules()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var unbalanced = fixture.AddTransaction("TX-DUP", new DateOnly(2026, 5, 10));
        fixture.AddLine(unbalanced, "1", "1111", "D", 100m);
        fixture.AddLine(unbalanced, "2", "1211", "C", 90m);
        var duplicate = fixture.AddTransaction("tx-dup", new DateOnly(2026, 5, 11));
        fixture.AddLine(duplicate, "3", "1111", "D", 50m);
        fixture.AddLine(duplicate, "4", "1211", "C", 50m);
        var invalid = fixture.AddTransaction("TX-INVALID", new DateOnly(2026, 7, 1));
        fixture.AddLine(invalid, "5", "1111", "X", -25m);
        var outside = fixture.AddTransaction("TX-OUTSIDE", new DateOnly(2027, 1, 1));
        fixture.AddLine(outside, "6", "1111", "D", 20m);
        fixture.AddLine(outside, "7", "1211", "C", 20m);
        await fixture.Db.SaveChangesAsync();

        var findings = await new AccountingAnomalyService(fixture.Db).EvaluateAsync(fixture.Import.Id, fixture.Import.StartDate, fixture.Import.EndDate);
        var rules = findings.Select(x => x.RuleId).ToHashSet();

        Assert.Contains("ACC-001", rules);
        Assert.Contains("ACC-002", rules);
        Assert.Contains("ACC-003", rules);
        Assert.Contains("ACC-004", rules);
        Assert.Contains("ACC-005", rules);
        Assert.All(findings, finding => Assert.False(string.IsNullOrWhiteSpace(finding.Reference)));
    }

    [Fact]
    public async Task EvaluateAsync_DoesNotMixDifferentSaftImports()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var valid = fixture.AddTransaction("TX-VALID", new DateOnly(2026, 4, 1));
        fixture.AddLine(valid, "1", "1111", "D", 10m);
        fixture.AddLine(valid, "2", "1211", "C", 10m);
        var otherImport = new SaftImport { DossierId = fixture.Import.DossierId, OriginalFileName = "other.xml", SaftVersion = "1.04_01", StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31), ImportedAtUtc = DateTime.UtcNow };
        fixture.Db.SaftImports.Add(otherImport);
        await fixture.Db.SaveChangesAsync();
        var invalidOther = new SaftTransaction { SaftImportId = otherImport.Id, JournalId = "G", TransactionId = "OTHER", Period = 1, TransactionDate = new DateOnly(2026, 4, 1), SourceId = "test", Description = "Other import", TransactionType = "N", GlPostingDate = new DateOnly(2026, 4, 1) };
        fixture.Db.SaftTransactions.Add(invalidOther);
        await fixture.Db.SaveChangesAsync();
        fixture.Db.SaftTransactionLines.Add(new SaftTransactionLine { SaftTransactionId = invalidOther.Id, RecordId = "O1", AccountId = "1111", Description = "Invalid", Side = "X", Amount = -99m });
        await fixture.Db.SaveChangesAsync();

        var findings = await new AccountingAnomalyService(fixture.Db).EvaluateAsync(fixture.Import.Id, fixture.Import.StartDate, fixture.Import.EndDate);

        Assert.Empty(findings);
    }

    [Fact]
    public async Task EvaluateAsync_IgnoresSubCentBalancingDifference()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var transaction = fixture.AddTransaction("TX-TOL", new DateOnly(2026, 3, 1));
        fixture.AddLine(transaction, "1", "1111", "D", 100m);
        fixture.AddLine(transaction, "2", "1211", "C", 99.995m);
        await fixture.Db.SaveChangesAsync();

        var findings = await new AccountingAnomalyService(fixture.Db).EvaluateAsync(fixture.Import.Id, fixture.Import.StartDate, fixture.Import.EndDate);

        Assert.DoesNotContain(findings, x => x.RuleId == "ACC-001");
    }

    private sealed class TestFixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection;
        private TestFixture(SqliteConnection connection, TabulariusDbContext db, SaftImport import) { this.connection = connection; Db = db; Import = import; }
        public TabulariusDbContext Db { get; }
        public SaftImport Import { get; }

        public static async Task<TestFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<TabulariusDbContext>().UseSqlite(connection).Options;
            var db = new TabulariusDbContext(options);
            await db.Database.EnsureCreatedAsync();
            var entity = new AccountingEntity { Name = "Synthetic Entity", TaxRegistrationNumber = "999999990", CreatedAtUtc = DateTime.UtcNow };
            db.AccountingEntities.Add(entity); await db.SaveChangesAsync();
            var dossier = new AnalysisDossier { AccountingEntityId = entity.Id, Name = "Synthetic 2026", FiscalYear = 2026, CreatedAtUtc = DateTime.UtcNow };
            db.AnalysisDossiers.Add(dossier); await db.SaveChangesAsync();
            var import = new SaftImport { DossierId = dossier.Id, OriginalFileName = "synthetic.xml", SaftVersion = "1.04_01", StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31), ImportedAtUtc = DateTime.UtcNow };
            db.SaftImports.Add(import); await db.SaveChangesAsync();
            return new TestFixture(connection, db, import);
        }

        public SaftTransaction AddTransaction(string transactionId, DateOnly date)
        {
            var transaction = new SaftTransaction { SaftImportId = Import.Id, JournalId = "G", JournalDescription = "General", TransactionId = transactionId, Period = date.Month, TransactionDate = date, SourceId = "test", Description = "Synthetic transaction", TransactionType = "N", GlPostingDate = date };
            Db.SaftTransactions.Add(transaction); Db.SaveChanges(); return transaction;
        }

        public void AddLine(SaftTransaction transaction, string recordId, string accountId, string side, decimal amount) => Db.SaftTransactionLines.Add(new SaftTransactionLine { SaftTransactionId = transaction.Id, RecordId = recordId, AccountId = accountId, Description = "Synthetic line", Side = side, Amount = amount });
        public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await connection.DisposeAsync(); }
    }
}
