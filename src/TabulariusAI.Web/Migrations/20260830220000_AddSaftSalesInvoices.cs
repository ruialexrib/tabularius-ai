using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TabulariusAI.Web.Data;

#nullable disable
namespace TabulariusAI.Web.Migrations;

[DbContext(typeof(TabulariusDbContext))]
[Migration("20260830220000_AddSaftSalesInvoices")]
public partial class AddSaftSalesInvoices : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "SaftSalesInvoices", columns: table => new { Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1").Annotation("Sqlite:Autoincrement", true), SaftImportId = table.Column<int>(nullable: false), InvoiceNo = table.Column<string>(maxLength: 100, nullable: false), InvoiceStatus = table.Column<string>(maxLength: 10, nullable: false), InvoiceDate = table.Column<DateOnly>(nullable: false), InvoiceType = table.Column<string>(maxLength: 10, nullable: false), SourceId = table.Column<string>(maxLength: 100, nullable: false), CustomerId = table.Column<string>(maxLength: 100, nullable: true), SystemEntryDate = table.Column<DateTime>(nullable: true), TaxPayable = table.Column<decimal>(precision: 19, scale: 4, nullable: false), NetTotal = table.Column<decimal>(precision: 19, scale: 4, nullable: false), GrossTotal = table.Column<decimal>(precision: 19, scale: 4, nullable: false) }, constraints: table => { table.PrimaryKey("PK_SaftSalesInvoices", x => x.Id); table.ForeignKey(name: "FK_SaftSalesInvoices_SaftImports_SaftImportId", column: x => x.SaftImportId, principalTable: "SaftImports", principalColumn: "Id", onDelete: ReferentialAction.Cascade); });
        migrationBuilder.CreateTable(name: "SaftSalesInvoiceLines", columns: table => new { Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1").Annotation("Sqlite:Autoincrement", true), SaftSalesInvoiceId = table.Column<int>(nullable: false), LineNumber = table.Column<string>(maxLength: 100, nullable: false), ProductCode = table.Column<string>(maxLength: 100, nullable: false), ProductDescription = table.Column<string>(maxLength: 500, nullable: false), Quantity = table.Column<decimal>(precision: 19, scale: 4, nullable: false), UnitOfMeasure = table.Column<string>(maxLength: 50, nullable: false), UnitPrice = table.Column<decimal>(precision: 19, scale: 4, nullable: false), TaxPointDate = table.Column<DateOnly>(nullable: true), TaxType = table.Column<string>(maxLength: 20, nullable: true), TaxCode = table.Column<string>(maxLength: 20, nullable: true), TaxPercentage = table.Column<decimal>(precision: 9, scale: 4, nullable: true), DebitAmount = table.Column<decimal>(precision: 19, scale: 4, nullable: false), CreditAmount = table.Column<decimal>(precision: 19, scale: 4, nullable: false) }, constraints: table => { table.PrimaryKey("PK_SaftSalesInvoiceLines", x => x.Id); table.ForeignKey(name: "FK_SaftSalesInvoiceLines_SaftSalesInvoices_SaftSalesInvoiceId", column: x => x.SaftSalesInvoiceId, principalTable: "SaftSalesInvoices", principalColumn: "Id", onDelete: ReferentialAction.Cascade); });
        migrationBuilder.CreateIndex(name: "IX_SaftSalesInvoices_SaftImportId_InvoiceNo", table: "SaftSalesInvoices", columns: new[] { "SaftImportId", "InvoiceNo" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_SaftSalesInvoices_SaftImportId_InvoiceDate", table: "SaftSalesInvoices", columns: new[] { "SaftImportId", "InvoiceDate" });
        migrationBuilder.CreateIndex(name: "IX_SaftSalesInvoiceLines_SaftSalesInvoiceId_LineNumber", table: "SaftSalesInvoiceLines", columns: new[] { "SaftSalesInvoiceId", "LineNumber" }, unique: true);
    }
    protected override void Down(MigrationBuilder migrationBuilder) { migrationBuilder.DropTable(name: "SaftSalesInvoiceLines"); migrationBuilder.DropTable(name: "SaftSalesInvoices"); }
}
