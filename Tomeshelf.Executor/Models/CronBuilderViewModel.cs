namespace Tomeshelf.Executor.Models;

/// <summary>
///     View model used by the cron builder partial to generate the Cron expression.
/// </summary>
public sealed class CronBuilderViewModel
{
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