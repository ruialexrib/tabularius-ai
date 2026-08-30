using System.ComponentModel.DataAnnotations;

namespace TabulariusAI.Web.Models;

public sealed class AiSettingsViewModel
{
    public bool IsEnabled { get; set; }
    [Required] public string Provider { get; set; } = "Ollama";
    [Required, Url] public string Endpoint { get; set; } = "http://localhost:11434";
    [Required] public string Model { get; set; } = "mistral";
    [DataType(DataType.Password)] public string? ApiKey { get; set; }
    [Range(0,2)] public decimal Temperature { get; set; } = 0.1m;
    [Range(10,600)] public int TimeoutSeconds { get; set; } = 120;
    [Required] public string SystemPrompt { get; set; } = string.Empty;
    [Required, StringLength(500)] public string HomePrompt { get; set; } = string.Empty;
    [Range(1,10080)] public int HomeCacheMinutes { get; set; } = 60;
    public bool HasApiKey { get; set; }
}
