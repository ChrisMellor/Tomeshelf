namespace Tomeshelf.Application.Contracts;

/// <summary>
///     Sleep routine summary for the day.
/// </summary>
public sealed class FitbitSleepSummaryDto
{
    public double? TotalSleepHours { get; init; }

    public double? TotalAwakeHours { get; init; }

    public double? EfficiencyPercentage { get; init; }

    public string Bedtime { get; init; }

    public string WakeTime { get; init; }

    public FitbitSleepLevelsDto Levels { get; init; } = new FitbitSleepLevelsDto();
}