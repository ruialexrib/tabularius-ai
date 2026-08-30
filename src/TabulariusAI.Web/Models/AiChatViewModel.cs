using System.ComponentModel.DataAnnotations;
namespace TabulariusAI.Web.Models;
public sealed class AiChatViewModel
{
    public int DossierId { get; set; }
    public string DossierName { get; set; } = string.Empty;
    [Required] public string Question { get; set; } = string.Empty;
    public string? Answer { get; set; }
}
