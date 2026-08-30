using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TabulariusAI.Web.Data;

#nullable disable

namespace TabulariusAI.Web.Migrations;

[DbContext(typeof(TabulariusDbContext))]
[Migration("20260831020000_AddSaftPayments")]
public partial class AddSaftPayments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var sqlite=migrationBuilder.ActiveProvider.Contains("Sqlite");
        migrationBuilder.CreateTable(name:"SaftPayments",columns:table=>new
        {
            Id=table.Column<int>(type:sqlite?"INTEGER":"int",nullable:false).Annotation("Sqlite:Autoincrement",true).Annotation("SqlServer:Identity","1, 1"),
            SaftImportId=table.Column<int>(type:sqlite?"INTEGER":"int",nullable:false),
            PaymentRefNo=table.Column<string>(type:sqlite?"TEXT":"nvarchar(100)",maxLength:100,nullable:false),
            TransactionDate=table.Column<DateOnly>(type:sqlite?"TEXT":"date",nullable:false),
            PaymentType=table.Column<string>(type:sqlite?"TEXT":"nvarchar(10)",maxLength:10,nullable:false),
            Description=table.Column<string>(type:sqlite?"TEXT":"nvarchar(500)",maxLength:500,nullable:true),
            SourceId=table.Column<string>(type:sqlite?"TEXT":"nvarchar(100)",maxLength:100,nullable:false),
            CustomerId=table.Column<string>(type:sqlite?"TEXT":"nvarchar(100)",maxLength:100,nullable:true),
            SystemEntryDate=table.Column<DateTime>(type:sqlite?"TEXT":"datetime2",nullable:true),
            PaymentStatus=table.Column<string>(type:sqlite?"TEXT":"nvarchar(10)",maxLength:10,nullable:false),
            TaxPayable=table.Column<decimal>(type:sqlite?"TEXT":"decimal(19,4)",precision:19,scale:4,nullable:false),
            NetTotal=table.Column<decimal>(type:sqlite?"TEXT":"decimal(19,4)",precision:19,scale:4,nullable:false),
            GrossTotal=table.Column<decimal>(type:sqlite?"TEXT":"decimal(19,4)",precision:19,scale:4,nullable:false)
        },constraints:table=>{table.PrimaryKey("PK_SaftPayments",x=>x.Id);table.ForeignKey("FK_SaftPayments_SaftImports_SaftImportId",x=>x.SaftImportId,"SaftImports","Id",onDelete:ReferentialAction.Cascade);});
        migrationBuilder.CreateTable(name:"SaftPaymentLines",columns:table=>new
        {
            Id=table.Column<int>(type:sqlite?"INTEGER":"int",nullable:false).Annotation("Sqlite:Autoincrement",true).Annotation("SqlServer:Identity","1, 1"),
            SaftPaymentId=table.Column<int>(type:sqlite?"INTEGER":"int",nullable:false),
            LineNumber=table.Column<string>(type:sqlite?"TEXT":"nvarchar(100)",maxLength:100,nullable:false),
            OriginatingOn=table.Column<string>(type:sqlite?"TEXT":"nvarchar(100)",maxLength:100,nullable:true),
            InvoiceDate=table.Column<DateOnly>(type:sqlite?"TEXT":"date",nullable:true),
            Description=table.Column<string>(type:sqlite?"TEXT":"nvarchar(500)",maxLength:500,nullable:true),
            DebitAmount=table.Column<decimal>(type:sqlite?"TEXT":"decimal(19,4)",precision:19,scale:4,nullable:false),
            CreditAmount=table.Column<decimal>(type:sqlite?"TEXT":"decimal(19,4)",precision:19,scale:4,nullable:false)
        },constraints:table=>{table.PrimaryKey("PK_SaftPaymentLines",x=>x.Id);table.ForeignKey("FK_SaftPaymentLines_SaftPayments_SaftPaymentId",x=>x.SaftPaymentId,"SaftPayments","Id",onDelete:ReferentialAction.Cascade);});
        migrationBuilder.CreateIndex("IX_SaftPayments_SaftImportId_PaymentRefNo","SaftPayments",new[]{"SaftImportId","PaymentRefNo"},unique:true);
        migrationBuilder.CreateIndex("IX_SaftPayments_SaftImportId_TransactionDate","SaftPayments",new[]{"SaftImportId","TransactionDate"});
        migrationBuilder.CreateIndex("IX_SaftPaymentLines_SaftPaymentId_LineNumber","SaftPaymentLines",new[]{"SaftPaymentId","LineNumber"},unique:true);
    }
    protected override void Down(MigrationBuilder migrationBuilder){migrationBuilder.DropTable("SaftPaymentLines");migrationBuilder.DropTable("SaftPayments");}
}
