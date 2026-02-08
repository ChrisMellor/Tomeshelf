using System.ComponentModel.DataAnnotations;
using Tomeshelf.Executor.Validation;

namespace Tomeshelf.Executor.Models;

public sealed class EndpointEditorModel
{
    [Required]
    [Display(Name = "Name")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Target URL")]
    [AbsoluteUrl]
    public string Url { get; set; } = string.Empty;

    [Required]
    [Display(Name = "HTTP Method")]
    public string Method { get; set; } = "POST";

    [Required]
    [Display(Name = "Cron Expression")]
    public string Cron { get; set; } = string.Empty;

    [Display(Name = "Enabled")]
    public bool Enabled { get; set; } = true;

    [Display(Name = "Headers (key:value per line)")]
    public string? Headers { get; set; }
}