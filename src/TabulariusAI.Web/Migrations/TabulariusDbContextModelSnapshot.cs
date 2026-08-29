using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using TabulariusAI.Web.Data;

#nullable disable

namespace TabulariusAI.Web.Migrations;

/// <summary>
/// Represents the Entity Framework Core model snapshot for the local Tabularius database.
/// </summary>
[DbContext(typeof(TabulariusDbContext))]
public partial class TabulariusDbContextModelSnapshot : ModelSnapshot
{
    /// <inheritdoc />
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "9.0.0").HasAnnotation("Relational:MaxIdentifierLength", 128);
        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity("TabulariusAI.Web.Data.Entities.AccountingEntity", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int"); SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
            b.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime2");
            b.Property<string>("Name").IsRequired().HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.Property<string>("TaxRegistrationNumber").IsRequired().HasMaxLength(20).HasColumnType("nvarchar(20)");
            b.HasKey("Id"); b.HasIndex("TaxRegistrationNumber").IsUnique(); b.ToTable("AccountingEntities");
        });

        modelBuilder.Entity("TabulariusAI.Web.Data.Entities.AnalysisDossier", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int"); SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
            b.Property<int>("AccountingEntityId").HasColumnType("int"); b.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime2");
            b.Property<int>("FiscalYear").HasColumnType("int"); b.Property<string>("Name").IsRequired().HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.HasKey("Id"); b.HasIndex("AccountingEntityId", "FiscalYear").IsUnique(); b.ToTable("AnalysisDossiers");
        });

        modelBuilder.Entity("TabulariusAI.Web.Data.Entities.SaftImport", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int"); SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
            b.Property<int>("DossierId").HasColumnType("int"); b.Property<DateOnly?>("EndDate").HasColumnType("date"); b.Property<DateTime>("ImportedAtUtc").HasColumnType("datetime2");
            b.Property<string>("OriginalFileName").IsRequired().HasMaxLength(260).HasColumnType("nvarchar(260)"); b.Property<string>("SaftVersion").IsRequired().HasMaxLength(30).HasColumnType("nvarchar(30)"); b.Property<DateOnly?>("StartDate").HasColumnType("date");
            b.HasKey("Id"); b.HasIndex("DossierId"); b.ToTable("SaftImports");
        });

        modelBuilder.Entity("TabulariusAI.Web.Data.Entities.SaftAccount", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int"); SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
            b.Property<string>("AccountId").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<decimal>("ClosingCreditBalance").HasPrecision(19, 4).HasColumnType("decimal(19,4)"); b.Property<decimal>("ClosingDebitBalance").HasPrecision(19, 4).HasColumnType("decimal(19,4)");
            b.Property<string>("Description").IsRequired().HasMaxLength(500).HasColumnType("nvarchar(500)"); b.Property<decimal>("OpeningCreditBalance").HasPrecision(19, 4).HasColumnType("decimal(19,4)"); b.Property<decimal>("OpeningDebitBalance").HasPrecision(19, 4).HasColumnType("decimal(19,4)");
            b.Property<int>("SaftImportId").HasColumnType("int"); b.Property<string>("TaxonomyReference").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.HasKey("Id"); b.HasIndex("SaftImportId", "AccountId").IsUnique(); b.ToTable("SaftAccounts");
        });

        modelBuilder.Entity("TabulariusAI.Web.Data.Entities.AnalysisDossier", b => { b.HasOne("TabulariusAI.Web.Data.Entities.AccountingEntity", "AccountingEntity").WithMany("Dossiers").HasForeignKey("AccountingEntityId").OnDelete(DeleteBehavior.Cascade).IsRequired(); b.Navigation("AccountingEntity"); });
        modelBuilder.Entity("TabulariusAI.Web.Data.Entities.SaftImport", b => { b.HasOne("TabulariusAI.Web.Data.Entities.AnalysisDossier", "Dossier").WithMany("Imports").HasForeignKey("DossierId").OnDelete(DeleteBehavior.Cascade).IsRequired(); b.Navigation("Dossier"); });
        modelBuilder.Entity("TabulariusAI.Web.Data.Entities.SaftAccount", b => { b.HasOne("TabulariusAI.Web.Data.Entities.SaftImport", "SaftImport").WithMany("Accounts").HasForeignKey("SaftImportId").OnDelete(DeleteBehavior.Cascade).IsRequired(); b.Navigation("SaftImport"); });
        modelBuilder.Entity("TabulariusAI.Web.Data.Entities.AccountingEntity", b => b.Navigation("Dossiers"));
        modelBuilder.Entity("TabulariusAI.Web.Data.Entities.AnalysisDossier", b => b.Navigation("Imports"));
        modelBuilder.Entity("TabulariusAI.Web.Data.Entities.SaftImport", b => b.Navigation("Accounts"));
    }
}
