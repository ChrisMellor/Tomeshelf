using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace Tomeshelf.Executor.Configuration;

public class ExecutorOptions
{
    public const string SectionName = "Executor";

    /// <summary>
    /// Enables or disables the scheduler entirely.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Collection of endpoint triggers to register.
    /// </summary>
    [Required]
    public List<EndpointScheduleOptions> Endpoints { get; set; } = [];
}

public class EndpointScheduleOptions
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Url]
    public string Url { get; set; } = string.Empty;

    [Required]
    public string Cron { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public string Method { get; set; } = HttpMethod.Post.Method;

    /// <summary>
    /// Optional TimeZone identifier understood by Quartz (defaults to UTC).
    /// </summary>
    public string? TimeZone { get; set; }

    public Dictionary<string, string>? Headers { get; set; }
}
