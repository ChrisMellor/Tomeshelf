using System;
using System.Collections.Generic;
using System.Linq;
using Tomeshelf.Executor.Configuration;

namespace Tomeshelf.Executor.Services;

internal static class ExecutorOptionsExtensions
{
    /// <summary>
    ///     Clones.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The result of the operation.</returns>
    public static ExecutorOptions Clone(this ExecutorOptions source)
    {
        var clone = new ExecutorOptions
        {
            Enabled = source.Enabled,
            Endpoints = source.Endpoints
                              .Select(Clone)
                              .ToList()
        };

        return clone;
    }

    /// <summary>
    ///     Clones.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The result of the operation.</returns>
    private static EndpointScheduleOptions Clone(EndpointScheduleOptions source)
    {
        return new EndpointScheduleOptions
        {
            Name = source.Name,
            Url = source.Url,
            Method = source.Method,
            Cron = source.Cron,
            Enabled = source.Enabled,
            TimeZone = source.TimeZone,
            Headers = source.Headers is null
                ? null
                : new Dictionary<string, string>(source.Headers, StringComparer.OrdinalIgnoreCase)
        };
    }
}