namespace TabulariusAI.Web.Models;

public sealed class HomeDashboardViewModel
{
    public int Entities { get; init; }
    public int Dossiers { get; init; }
    public int Imports { get; init; }
    public int Accounts { get; init; }
    public int Transactions { get; init; }
    public int SalesDocuments { get; init; }
    public decimal GrossSales { get; init; }
    public bool AiEnabled { get; init; }
    public string AiProvider { get; init; } = string.Empty;
    public string AiModel { get; init; } = string.Empty;
    public string AiWelcomePrompt { get; init; } = string.Empty;
    public IReadOnlyList<HomeDossierSummary> DossierSummaries { get; init; } = [];
    public IReadOnlyList<HomeEvolutionPoint> Evolution { get; init; } = [];
}

public sealed record HomeDossierSummary(int Id,string Entity,string Name,int FiscalYear,int Imports,int Transactions,int SalesDocuments,decimal GrossSales);
public sealed record HomeEvolutionPoint(int FiscalYear,int Transactions,decimal GrossSales);
