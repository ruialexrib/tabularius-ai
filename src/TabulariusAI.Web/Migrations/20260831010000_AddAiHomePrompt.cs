using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TabulariusAI.Web.Data;
#nullable disable
namespace TabulariusAI.Web.Migrations;
[DbContext(typeof(TabulariusDbContext))]
[Migration("20260831010000_AddAiHomePrompt")]
public partial class AddAiHomePrompt : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)=>migrationBuilder.AddColumn<string>(name:"HomePrompt",table:"AiSettings",nullable:false,defaultValue:"Escolha um dossier para explorar os dados contabilísticos com o assistente AI, ou importe um novo SAF-T (PT) para acrescentar informação à análise.");
    protected override void Down(MigrationBuilder migrationBuilder)=>migrationBuilder.DropColumn(name:"HomePrompt",table:"AiSettings");
}
