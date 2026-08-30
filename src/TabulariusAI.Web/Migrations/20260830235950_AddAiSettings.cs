using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TabulariusAI.Web.Data;
#nullable disable
namespace TabulariusAI.Web.Migrations;
[DbContext(typeof(TabulariusDbContext))]
[Migration("20260830235950_AddAiSettings")]
public partial class AddAiSettings : Migration
{
 protected override void Up(MigrationBuilder migrationBuilder)=>migrationBuilder.CreateTable(name:"AiSettings",columns:table=>new{Id=table.Column<int>(nullable:false).Annotation("SqlServer:Identity","1, 1").Annotation("Sqlite:Autoincrement",true),IsEnabled=table.Column<bool>(nullable:false),Provider=table.Column<string>(maxLength:30,nullable:false),Endpoint=table.Column<string>(maxLength:500,nullable:false),Model=table.Column<string>(maxLength:200,nullable:false),ApiKey=table.Column<string>(maxLength:1000,nullable:true),Temperature=table.Column<decimal>(precision:3,scale:2,nullable:false),TimeoutSeconds=table.Column<int>(nullable:false),SystemPrompt=table.Column<string>(nullable:false),UpdatedAtUtc=table.Column<DateTime>(nullable:false)},constraints:table=>table.PrimaryKey("PK_AiSettings",x=>x.Id));
 protected override void Down(MigrationBuilder migrationBuilder)=>migrationBuilder.DropTable(name:"AiSettings");
}
