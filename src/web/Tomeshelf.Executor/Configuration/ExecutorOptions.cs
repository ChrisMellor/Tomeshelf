using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tomeshelf.Executor.Configuration;

public class ExecutorOptions
{
    public const string SectionName = "Executor";

    /// <summary>
    ///     Enables or disables the scheduler entirely.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     Collection of endpoint triggers to register.
    /// </summary>
    [Required]
    public List<EndpointScheduleOptions> Endpoints { get; set; } = [];
}