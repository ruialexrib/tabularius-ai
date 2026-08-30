namespace TabulariusAI.Web.Data.Entities;

/// <summary>Stores administrator-managed AI provider and assistant configuration.</summary>
public sealed class AiSettings
{
    public int Id { get; set; }
    public bool IsEnabled { get; set; }
    public string Provider { get; set; } = "Ollama";
    public string Endpoint { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "mistral";
    public string? ApiKey { get; set; }
    public decimal Temperature { get; set; } = 0.1m;
    public int TimeoutSeconds { get; set; } = 120;
    public string SystemPrompt { get; set; } = "És o assistente de análise contabilística do Tabularius AI. Responde em português europeu. Para qualquer afirmação sobre dados do dossier utiliza exclusivamente as tools disponibilizadas. Nunca inventes valores, documentos, contas, clientes, fornecedores ou factos ausentes dos resultados das tools. Quando não existirem dados suficientes, indica claramente essa limitação.";
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
