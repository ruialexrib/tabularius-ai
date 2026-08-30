using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TabulariusAI.Web.Data;

#nullable disable

namespace TabulariusAI.Web.Migrations;

/// <summary>Adds source-traceable SAF-T (PT) ledger accounts to the local workspace.</summary>
[DbContext(typeof(TabulariusDbContext))]
[Migration("20260829180000_AddSaftAccounts")]
public partial class AddSaftAccounts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_AnalysisDossiers_AccountingEntityId", table: "AnalysisDossiers");
        migrationBuilder.CreateIndex(name: "IX_AnalysisDossiers_AccountingEntityId_FiscalYear", table: "AnalysisDossiers", columns: new[] { "AccountingEntityId", "FiscalYear" }, unique: true);
        migrationBuilder.CreateTable(name: "SaftAccounts", columns: table => new
        {
            Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1").Annotation("Sqlite:Autoincrement", true),
            SaftImportId = table.Column<int>(nullable: false),
            AccountId = table.Column<string>(maxLength: 100, nullable: false),
            Description = table.Column<string>(maxLength: 500, nullable: false),
            OpeningDebitBalance = table.Column<decimal>(precision: 19, scale: 4, nullable: false),
            OpeningCreditBalance = table.Column<decimal>(precision: 19, scale: 4, nullable: false),
            ClosingDebitBalance = table.Column<decimal>(precision: 19, scale: 4, nullable: false),
            ClosingCreditBalance = table.Column<decimal>(precision: 19, scale: 4, nullable: false),
            TaxonomyReference = table.Column<string>(maxLength: 100, nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_SaftAccounts", x => x.Id);
            table.ForeignKey(name: "FK_SaftAccounts_SaftImports_SaftImportId", column: x => x.SaftImportId, principalTable: "SaftImports", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        });
        migrationBuilder.CreateIndex(name: "IX_SaftAccounts_SaftImportId_AccountId", table: "SaftAccounts", columns: new[] { "SaftImportId", "AccountId" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "SaftAccounts");
        migrationBuilder.DropIndex(name: "IX_AnalysisDossiers_AccountingEntityId_FiscalYear", table: "AnalysisDossiers");
        migrationBuilder.CreateIndex(name: "IX_AnalysisDossiers_AccountingEntityId", table: "AnalysisDossiers", column: "AccountingEntityId");
    }
}
