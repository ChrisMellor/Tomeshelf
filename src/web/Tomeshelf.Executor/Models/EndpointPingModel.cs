using System.ComponentModel.DataAnnotations;
using Tomeshelf.Executor.Validation;

namespace Tomeshelf.Executor.Models;

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