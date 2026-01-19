namespace Tomeshelf.Application.Contracts.Fitbit;

/// <summary>
///     Breakdown of sleep stages for the night.
/// </summary>
public sealed record FitbitSleepLevelsDto
{
    public int? DeepMinutes { get; init; }

    public int? LightMinutes { get; init; }

    public int? RemMinutes { get; init; }

    public int? WakeMinutes { get; init; }
}