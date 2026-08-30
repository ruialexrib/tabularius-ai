using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TabulariusAI.Web.Data;

#nullable disable
namespace TabulariusAI.Web.Migrations;

/// <summary>Adds persisted SAF-T (PT) MasterFiles tax table entries.</summary>
[DbContext(typeof(TabulariusDbContext))]
[Migration("20260830235930_AddSaftTaxTable")]
public partial class AddSaftTaxTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name:"SaftTaxEntries",columns:table=>new{Id=table.Column<int>(nullable:false).Annotation("SqlServer:Identity","1, 1").Annotation("Sqlite:Autoincrement",true),SaftImportId=table.Column<int>(nullable:false),TaxType=table.Column<string>(maxLength:20,nullable:false),TaxCountryRegion=table.Column<string>(maxLength:10,nullable:false),TaxCode=table.Column<string>(maxLength:20,nullable:false),Description=table.Column<string>(maxLength:500,nullable:false),TaxExpirationDate=table.Column<DateOnly>(nullable:true),TaxPercentage=table.Column<decimal>(precision:9,scale:4,nullable:true),TaxAmount=table.Column<decimal>(precision:19,scale:4,nullable:true)},constraints:table=>{table.PrimaryKey("PK_SaftTaxEntries",x=>x.Id);table.ForeignKey("FK_SaftTaxEntries_SaftImports_SaftImportId",x=>x.SaftImportId,"SaftImports","Id",onDelete:ReferentialAction.Cascade);});
        migrationBuilder.CreateIndex(name:"IX_SaftTaxEntries_SaftImportId_TaxType_TaxCountryRegion_TaxCode",table:"SaftTaxEntries",columns:new[]{"SaftImportId","TaxType","TaxCountryRegion","TaxCode"},unique:true);
    }
    protected override void Down(MigrationBuilder migrationBuilder)=>migrationBuilder.DropTable(name:"SaftTaxEntries");
}
