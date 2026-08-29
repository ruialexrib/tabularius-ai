using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TabulariusAI.Web.Data;

#nullable disable

namespace TabulariusAI.Web.Migrations;

/// <summary>
/// Creates the initial local accounting workspace schema.
/// </summary>
[DbContext(typeof(TabulariusDbContext))]
[Migration("20260829150000_InitialAccountingWorkspace")]
public partial class InitialAccountingWorkspace : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AccountingEntities",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                TaxRegistrationNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AccountingEntities", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AnalysisDossiers",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AccountingEntityId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                FiscalYear = table.Column<int>(type: "int", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AnalysisDossiers", x => x.Id);
                table.ForeignKey(
                    name: "FK_AnalysisDossiers_AccountingEntities_AccountingEntityId",
                    column: x => x.AccountingEntityId,
                    principalTable: "AccountingEntities",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "SaftImports",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DossierId = table.Column<int>(type: "int", nullable: false),
                OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                SaftVersion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                ImportedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SaftImports", x => x.Id);
                table.ForeignKey(
                    name: "FK_SaftImports_AnalysisDossiers_DossierId",
                    column: x => x.DossierId,
                    principalTable: "AnalysisDossiers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AccountingEntities_TaxRegistrationNumber",
            table: "AccountingEntities",
            column: "TaxRegistrationNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AnalysisDossiers_AccountingEntityId",
            table: "AnalysisDossiers",
            column: "AccountingEntityId");

        migrationBuilder.CreateIndex(
            name: "IX_SaftImports_DossierId",
            table: "SaftImports",
            column: "DossierId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "SaftImports");
        migrationBuilder.DropTable(name: "AnalysisDossiers");
        migrationBuilder.DropTable(name: "AccountingEntities");
    }
}
