using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace TabulariusAI.Web.Migrations;
public partial class AddVatAnalysisPrompt:Migration
{
protected override void Up(MigrationBuilder migrationBuilder)=>migrationBuilder.AddColumn<string>(name:"VatAnalysisPrompt",table:"AiSettings",type:"TEXT",nullable:false,defaultValue:"Interpreta o resumo de IVA por taxa e documento, destacando o efeito líquido dos documentos, taxas materialmente relevantes e situações de anulação ou compensação. Usa exclusivamente os valores fornecidos e não recalcules o imposto.");
protected override void Down(MigrationBuilder migrationBuilder)=>migrationBuilder.DropColumn(name:"VatAnalysisPrompt",table:"AiSettings");
}
