using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TabulariusAI.Web.Migrations;

public partial class AddAnalyticalAiPrompts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name:"AccountAnalysisPrompt",table:"AiSettings",type:"nvarchar(max)",nullable:false,defaultValue:"Analisa a distribuição dos movimentos por conta, identifica concentrações e contas que mereçam revisão e explica os padrões apenas com base nos dados fornecidos.");
        migrationBuilder.AddColumn<string>(name:"AnalyticsOverviewPrompt",table:"AiSettings",type:"nvarchar(max)",nullable:false,defaultValue:"Analisa os indicadores globais apresentados, destaca os factos mais relevantes, eventuais desequilíbrios e prioridades de revisão. Não recalcules nem inventes valores.");
        migrationBuilder.AddColumn<string>(name:"AnomaliesPrompt",table:"AiSettings",type:"nvarchar(max)",nullable:false,defaultValue:"Interpreta as anomalias determinísticas detetadas, prioriza as situações com maior impacto potencial e sugere verificações contabilísticas concretas. Não declares erro contabilístico sem evidência suficiente.");
        migrationBuilder.AddColumn<string>(name:"BalanceSheetPrompt",table:"AiSettings",type:"nvarchar(max)",nullable:false,defaultValue:"Interpreta o balanço sintético, destacando estrutura do ativo, capital próprio e passivo, equilíbrio e situações que mereçam revisão. Respeita as limitações da classificação apresentada.");
        migrationBuilder.AddColumn<string>(name:"IncomeStatementPrompt",table:"AiSettings",type:"nvarchar(max)",nullable:false,defaultValue:"Interpreta a demonstração de resultados, destacando composição de rendimentos e gastos, resultado do período e rubricas materialmente relevantes. Não extrapoles para períodos não fornecidos.");
        migrationBuilder.AddColumn<string>(name:"TrialBalancePrompt",table:"AiSettings",type:"nvarchar(max)",nullable:false,defaultValue:"Produz uma leitura profissional do balancete, destacando equilíbrio, movimentos materialmente relevantes e contas que justifiquem revisão. Usa exclusivamente os valores determinísticos fornecidos.");
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name:"AccountAnalysisPrompt",table:"AiSettings");migrationBuilder.DropColumn(name:"AnalyticsOverviewPrompt",table:"AiSettings");migrationBuilder.DropColumn(name:"AnomaliesPrompt",table:"AiSettings");migrationBuilder.DropColumn(name:"BalanceSheetPrompt",table:"AiSettings");migrationBuilder.DropColumn(name:"IncomeStatementPrompt",table:"AiSettings");migrationBuilder.DropColumn(name:"TrialBalancePrompt",table:"AiSettings");
    }
}
