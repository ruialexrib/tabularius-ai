using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data.Entities;

namespace TabulariusAI.Web.Data;

/// <summary>
/// Represents the local persistence context for accounting entities, dossiers and imports.
/// </summary>
public sealed class TabulariusDbContext(DbContextOptions<TabulariusDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the accounting entities managed by the application.
    /// </summary>
    public DbSet<AccountingEntity> AccountingEntities => Set<AccountingEntity>();

    /// <summary>
    /// Gets the analysis dossiers associated with accounting entities.
    /// </summary>
    public DbSet<AnalysisDossier> AnalysisDossiers => Set<AnalysisDossier>();

    /// <summary>
    /// Gets the SAF-T (PT) imports associated with analysis dossiers.
    /// </summary>
    public DbSet<SaftImport> SaftImports => Set<SaftImport>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountingEntity>(entity =>
        {
            entity.Property(item => item.Name).HasMaxLength(200).IsRequired();
            entity.Property(item => item.TaxRegistrationNumber).HasMaxLength(20).IsRequired();
            entity.HasIndex(item => item.TaxRegistrationNumber).IsUnique();
        });

        modelBuilder.Entity<AnalysisDossier>(entity =>
        {
            entity.Property(item => item.Name).HasMaxLength(200).IsRequired();
            entity.HasOne(item => item.AccountingEntity)
                .WithMany(item => item.Dossiers)
                .HasForeignKey(item => item.AccountingEntityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SaftImport>(entity =>
        {
            entity.Property(item => item.OriginalFileName).HasMaxLength(260).IsRequired();
            entity.Property(item => item.SaftVersion).HasMaxLength(30).IsRequired();
            entity.HasOne(item => item.Dossier)
                .WithMany(item => item.Imports)
                .HasForeignKey(item => item.DossierId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
