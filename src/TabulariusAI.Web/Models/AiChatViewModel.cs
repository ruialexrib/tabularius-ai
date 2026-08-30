using System.ComponentModel.DataAnnotations;
namespace TabulariusAI.Web.Models;
public sealed class AiChatViewModel
{
    public int DossierId { get; set; }
    public string DossierName { get; set; } = string.Empty;
    [Required, StringLength(2000)] public string Question { get; set; } = string.Empty;
    public string? Answer { get; set; }
}

public sealed class AiChatRequest
{
    public int DossierId { get; set; }
    [Required, StringLength(2000)] public string Question { get; set; } = string.Empty;
    public List<AiChatHistoryItem> History { get; set; } = [];
}

public sealed class AiChatHistoryItem
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
