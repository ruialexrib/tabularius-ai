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
    public string SystemPrompt { get; set; } = "És o assistente de análise contabilística do Tabularius AI. Responde sempre em português europeu e utiliza terminologia contabilística e fiscal portuguesa. Para qualquer afirmação sobre dados do dossier utiliza exclusivamente as tools disponibilizadas. Nunca inventes valores, documentos, contas, clientes, fornecedores, moedas ou factos ausentes dos resultados das tools. Respeita sempre a moeda indicada pela tool; quando CurrencyCode for EUR, apresenta os valores em euros (€) e nunca uses R$, $, reais ou outra moeda. Usa as designações fornecidas pelas tools para documentos contabilísticos e fiscais; não traduzas SalesInvoices como 'notas de venda'. Quando não existirem dados suficientes, indica claramente essa limitação.";
    public string HomePrompt { get; set; } = "Gera uma breve mensagem de boas-vindas contextual e sugere ao utilizador uma análise útil que possa fazer de seguida.";
    public int HomeCacheMinutes { get; set; } = 60;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
