using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TabulariusAI.Web.Data;

#nullable disable
namespace TabulariusAI.Web.Migrations;

/// <summary>Allows multiple SAF-T tax table entries with the same tax code, for example different rates or validity periods.</summary>
[DbContext(typeof(TabulariusDbContext))]
[Migration("20260830235940_AllowDuplicateSaftTaxCodes")]
public partial class AllowDuplicateSaftTaxCodes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name:"IX_SaftTaxEntries_SaftImportId_TaxType_TaxCountryRegion_TaxCode",table:"SaftTaxEntries");
        migrationBuilder.CreateIndex(name:"IX_SaftTaxEntries_SaftImportId",table:"SaftTaxEntries",column:"SaftImportId");
        migrationBuilder.CreateIndex(name:"IX_SaftTaxEntries_SaftImportId_TaxType_TaxCountryRegion_TaxCode",table:"SaftTaxEntries",columns:new[]{"SaftImportId","TaxType","TaxCountryRegion","TaxCode"});
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name:"IX_SaftTaxEntries_SaftImportId",table:"SaftTaxEntries");
        migrationBuilder.DropIndex(name:"IX_SaftTaxEntries_SaftImportId_TaxType_TaxCountryRegion_TaxCode",table:"SaftTaxEntries");
        migrationBuilder.CreateIndex(name:"IX_SaftTaxEntries_SaftImportId_TaxType_TaxCountryRegion_TaxCode",table:"SaftTaxEntries",columns:new[]{"SaftImportId","TaxType","TaxCountryRegion","TaxCode"},unique:true);
    }
}
