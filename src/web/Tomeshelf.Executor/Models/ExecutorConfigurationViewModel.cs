using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Tomeshelf.Executor.Validation;

namespace Tomeshelf.Executor.Models;

public sealed class ExecutorConfigurationViewModel
{
    public bool Enabled { get; set; }

    public List<EndpointSummaryViewModel> Endpoints { get; set; } = [];

    public EndpointEditorModel Editor { get; set; } = new EndpointEditorModel();

    public EndpointPingModel Ping { get; set; } = new EndpointPingModel();

    public EndpointPingResultViewModel? PingResult { get; set; }

    public List<ApiServiceOptionViewModel> ApiServices { get; set; } = [];
}

public sealed class EndpointSummaryViewModel
{
    public required string Name { get; init; }

    public required string Url { get; init; }

    public required string Method { get; init; }

    public required string Cron { get; init; }

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

public sealed class EndpointPingModel
{
    [Required]
    [Display(Name = "Target URL")]
    [AbsoluteUrl]
    public string Url { get; set; } = string.Empty;

    [Required]
    [Display(Name = "HTTP Method")]
    public string Method { get; set; } = "GET";

    [Display(Name = "Headers (key:value per line)")]
    public string? Headers { get; set; }
}

public sealed class EndpointPingResultViewModel
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public int? StatusCode { get; init; }

    public string? ResponseBody { get; init; }

    public TimeSpan Duration { get; init; }
}

public sealed class ApiServiceOptionViewModel
{
    public required string ServiceName { get; init; }

    public required string DisplayName { get; init; }

    public required string BaseAddress { get; init; }
}