using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TabulariusAI.Web.Data;

#nullable disable

namespace TabulariusAI.Web.Migrations;

[DbContext(typeof(TabulariusDbContext))]
[Migration("20260902000000_AddVatAnalysisPrompt")]
public partial class AddVatAnalysisPrompt : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var type = migrationBuilder.ActiveProvider.Contains("Sqlite") ? "TEXT" : "nvarchar(max)";
        migrationBuilder.AddColumn<string>(
            name: "VatAnalysisPrompt",
            table: "AiSettings",
            type: type,
            nullable: false,
            defaultValue: "Interpreta o resumo de IVA por taxa e documento, destacando o efeito líquido dos documentos, taxas materialmente relevantes e situações de anulação ou compensação. Usa exclusivamente os valores fornecidos e não recalcules o imposto.");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "VatAnalysisPrompt", table: "AiSettings");
    }
}
