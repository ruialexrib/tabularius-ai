using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TabulariusAI.Web.Data;

#nullable disable

namespace TabulariusAI.Web.Migrations;

/// <summary>Creates the initial local accounting workspace schema.</summary>
[DbContext(typeof(TabulariusDbContext))]
[Migration("20260829150000_InitialAccountingWorkspace")]
public partial class InitialAccountingWorkspace : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "AccountingEntities", columns: table => new
        {
            Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1").Annotation("Sqlite:Autoincrement", true),
            Name = table.Column<string>(maxLength: 200, nullable: false),
            TaxRegistrationNumber = table.Column<string>(maxLength: 20, nullable: false),
            CreatedAtUtc = table.Column<DateTime>(nullable: false)
        }, constraints: table => table.PrimaryKey("PK_AccountingEntities", x => x.Id));

        migrationBuilder.CreateTable(name: "AnalysisDossiers", columns: table => new
        {
            Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1").Annotation("Sqlite:Autoincrement", true),
            AccountingEntityId = table.Column<int>(nullable: false),
            Name = table.Column<string>(maxLength: 200, nullable: false),
            FiscalYear = table.Column<int>(nullable: false),
            CreatedAtUtc = table.Column<DateTime>(nullable: false)
        }, constraints: table => { table.PrimaryKey("PK_AnalysisDossiers", x => x.Id); table.ForeignKey("FK_AnalysisDossiers_AccountingEntities_AccountingEntityId", x => x.AccountingEntityId, "AccountingEntities", "Id", onDelete: ReferentialAction.Cascade); });

        migrationBuilder.CreateTable(name: "SaftImports", columns: table => new
        {
            Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1").Annotation("Sqlite:Autoincrement", true),
            DossierId = table.Column<int>(nullable: false),
            OriginalFileName = table.Column<string>(maxLength: 260, nullable: false),
            SaftVersion = table.Column<string>(maxLength: 30, nullable: false),
            StartDate = table.Column<DateOnly>(nullable: true),
            EndDate = table.Column<DateOnly>(nullable: true),
            ImportedAtUtc = table.Column<DateTime>(nullable: false)
        }, constraints: table => { table.PrimaryKey("PK_SaftImports", x => x.Id); table.ForeignKey("FK_SaftImports_AnalysisDossiers_DossierId", x => x.DossierId, "AnalysisDossiers", "Id", onDelete: ReferentialAction.Cascade); });

        migrationBuilder.CreateIndex("IX_AccountingEntities_TaxRegistrationNumber", "AccountingEntities", "TaxRegistrationNumber", unique: true);
        migrationBuilder.CreateIndex("IX_AnalysisDossiers_AccountingEntityId", "AnalysisDossiers", "AccountingEntityId");
        migrationBuilder.CreateIndex("IX_SaftImports_DossierId", "SaftImports", "DossierId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("SaftImports"); migrationBuilder.DropTable("AnalysisDossiers"); migrationBuilder.DropTable("AccountingEntities");
    }
}
