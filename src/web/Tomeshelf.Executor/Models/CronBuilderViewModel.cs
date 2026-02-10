using System;

namespace Tomeshelf.Executor.Models;

/// <summary>
///     View model used by the cron builder partial to generate the Cron expression.
/// </summary>
public sealed class CronBuilderViewModel
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CronBuilderViewModel" /> class.
    /// </summary>
    /// <param name="inputId">The input id.</param>
    /// <param name="inputName">The input name.</param>
    /// <param name="initialValue">The initial value.</param>
    public CronBuilderViewModel(string inputId, string inputName, string? initialValue)
    {
        InputId = inputId ?? throw new ArgumentNullException(nameof(inputId));
        InputName = inputName ?? throw new ArgumentNullException(nameof(inputName));
        InitialValue = initialValue ?? string.Empty;
    }

    public string InputId { get; }

    public string InputName { get; }

    public string InitialValue { get; }
}