using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data.Entities;

namespace TabulariusAI.Web.Data;

/// <summary>
/// Represents the local persistence context for accounting entities, dossiers and imports.
/// </summary>
public sealed class TabulariusDbContext(DbContextOptions<TabulariusDbContext> options) : DbContext(options)
{
    /// <summary>Gets the accounting entities managed by the application.</summary>
    public DbSet<AccountingEntity> AccountingEntities => Set<AccountingEntity>();
    /// <summary>Gets the analysis dossiers associated with accounting entities.</summary>
    public DbSet<AnalysisDossier> AnalysisDossiers => Set<AnalysisDossier>();
    /// <summary>Gets the SAF-T (PT) imports associated with analysis dossiers.</summary>
    public DbSet<SaftImport> SaftImports => Set<SaftImport>();
    /// <summary>Gets the ledger accounts preserved from SAF-T (PT) imports.</summary>
    public DbSet<SaftAccount> SaftAccounts => Set<SaftAccount>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AccountingEntity>(entity =>
        {
            entity.Property(item => item.Name).HasMaxLength(200).IsRequired();
            entity.Property(item => item.TaxRegistrationNumber).HasMaxLength(20).IsRequired();
            entity.HasIndex(item => item.TaxRegistrationNumber).IsUnique();
        });

        modelBuilder.Entity<AnalysisDossier>(entity =>
        {
            entity.Property(item => item.Name).HasMaxLength(200).IsRequired();
            entity.HasOne(item => item.AccountingEntity).WithMany(item => item.Dossiers)
                .HasForeignKey(item => item.AccountingEntityId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(item => new { item.AccountingEntityId, item.FiscalYear }).IsUnique();
        });

        modelBuilder.Entity<SaftImport>(entity =>
        {
            entity.Property(item => item.OriginalFileName).HasMaxLength(260).IsRequired();
            entity.Property(item => item.SaftVersion).HasMaxLength(30).IsRequired();
            entity.HasOne(item => item.Dossier).WithMany(item => item.Imports)
                .HasForeignKey(item => item.DossierId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SaftAccount>(entity =>
        {
            entity.Property(item => item.AccountId).HasMaxLength(100).IsRequired();
            entity.Property(item => item.Description).HasMaxLength(500).IsRequired();
            entity.Property(item => item.TaxonomyReference).HasMaxLength(100);
            entity.Property(item => item.OpeningDebitBalance).HasPrecision(19, 4);
            entity.Property(item => item.OpeningCreditBalance).HasPrecision(19, 4);
            entity.Property(item => item.ClosingDebitBalance).HasPrecision(19, 4);
            entity.Property(item => item.ClosingCreditBalance).HasPrecision(19, 4);
            entity.HasOne(item => item.SaftImport).WithMany(item => item.Accounts)
                .HasForeignKey(item => item.SaftImportId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(item => new { item.SaftImportId, item.AccountId }).IsUnique();
        });
    }
}
