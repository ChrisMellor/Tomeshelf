using System.ComponentModel.DataAnnotations;

namespace Tomeshelf.Executor.Models;

public sealed class ExecutorConfigurationViewModel
{
    public bool Enabled { get; set; }

    public List<EndpointSummaryViewModel> Endpoints { get; set; } = [];

    public EndpointEditorModel Editor { get; set; } = new();
}

public sealed class EndpointSummaryViewModel
{
    public required string Name { get; init; }

    public required string Url { get; init; }

    public required string Method { get; init; }

    public required string Cron { get; init; }

    public string? TimeZone { get; init; }

    public bool Enabled { get; init; }

    public string HeadersDisplay { get; init; } = string.Empty;
}

public sealed class EndpointEditorModel
{
    [Required]
    [Display(Name = "Name")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Target URL")]
    [Url]
    public string Url { get; set; } = string.Empty;

    [Required]
    [Display(Name = "HTTP Method")]
    public string Method { get; set; } = "POST";

    [Required]
    [Display(Name = "Cron Expression")]
    public string Cron { get; set; } = string.Empty;

    [Display(Name = "Time Zone")]
    public string? TimeZone { get; set; }

    [Display(Name = "Enabled")]
    public bool Enabled { get; set; } = true;

    [Display(Name = "Headers (key:value per line)")]
    public string? Headers { get; set; }
}
