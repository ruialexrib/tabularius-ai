using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TabulariusAI.Web.Data;

#nullable disable
namespace TabulariusAI.Web.Migrations;

/// <summary>Adds persisted SAF-T movement of goods documents and lines.</summary>
[DbContext(typeof(TabulariusDbContext))]
[Migration("20260830235500_AddSaftStockMovements")]
public partial class AddSaftStockMovements : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name:"SaftStockMovements",columns:table=>new{Id=table.Column<int>(nullable:false).Annotation("SqlServer:Identity","1, 1"),SaftImportId=table.Column<int>(nullable:false),DocumentNumber=table.Column<string>(maxLength:100,nullable:false),DocumentStatus=table.Column<string>(maxLength:10,nullable:false),MovementDate=table.Column<DateOnly>(nullable:false),MovementType=table.Column<string>(maxLength:10,nullable:false),SourceId=table.Column<string>(maxLength:100,nullable:false),CustomerId=table.Column<string>(maxLength:100,nullable:true),SupplierId=table.Column<string>(maxLength:100,nullable:true),SystemEntryDate=table.Column<DateTime>(nullable:true)},constraints:table=>{table.PrimaryKey("PK_SaftStockMovements",x=>x.Id);table.ForeignKey("FK_SaftStockMovements_SaftImports_SaftImportId",x=>x.SaftImportId,"SaftImports","Id",onDelete:ReferentialAction.Cascade);});
        migrationBuilder.CreateTable(name:"SaftStockMovementLines",columns:table=>new{Id=table.Column<int>(nullable:false).Annotation("SqlServer:Identity","1, 1"),SaftStockMovementId=table.Column<int>(nullable:false),LineNumber=table.Column<string>(maxLength:100,nullable:false),ProductCode=table.Column<string>(maxLength:100,nullable:false),ProductDescription=table.Column<string>(maxLength:500,nullable:false),Quantity=table.Column<decimal>(type:"decimal(19,4)",precision:19,scale:4,nullable:false),UnitOfMeasure=table.Column<string>(maxLength:50,nullable:false),UnitPrice=table.Column<decimal>(type:"decimal(19,4)",precision:19,scale:4,nullable:true)},constraints:table=>{table.PrimaryKey("PK_SaftStockMovementLines",x=>x.Id);table.ForeignKey("FK_SaftStockMovementLines_SaftStockMovements_SaftStockMovementId",x=>x.SaftStockMovementId,"SaftStockMovements","Id",onDelete:ReferentialAction.Cascade);});
        migrationBuilder.CreateIndex(name:"IX_SaftStockMovements_SaftImportId_DocumentNumber",table:"SaftStockMovements",columns:new[]{"SaftImportId","DocumentNumber"},unique:true); migrationBuilder.CreateIndex(name:"IX_SaftStockMovements_SaftImportId_MovementDate",table:"SaftStockMovements",columns:new[]{"SaftImportId","MovementDate"}); migrationBuilder.CreateIndex(name:"IX_SaftStockMovementLines_SaftStockMovementId_LineNumber",table:"SaftStockMovementLines",columns:new[]{"SaftStockMovementId","LineNumber"},unique:true);
    }
    protected override void Down(MigrationBuilder migrationBuilder){migrationBuilder.DropTable(name:"SaftStockMovementLines");migrationBuilder.DropTable(name:"SaftStockMovements");}
}
