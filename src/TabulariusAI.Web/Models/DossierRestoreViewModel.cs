using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TabulariusAI.Web.Models;

public sealed class DossierRestoreViewModel
{
    [Required(ErrorMessage = "Selecione um ficheiro de backup JSON.")]
    [Display(Name = "Ficheiro de backup")]
    public IFormFile? BackupFile { get; set; }
}
