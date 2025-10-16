using System;

namespace Tomeshelf.Application.Contracts;

/// <summary>
///     Aggregated snapshot for a Fitbit dashboard view.
/// </summary>
public sealed record FitbitDashboardDto
{
    /// <summary>
    ///     Gets or sets the ISO-8601 date (yyyy-MM-dd) the snapshot represents.
    /// </summary>
    public required string Date { get; init; }

    /// <summary>
    ///     Gets or sets the timestamp (UTC) when the snapshot was generated.
    /// </summary>
    public DateTimeOffset GeneratedUtc { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets or sets the weight summary.
    /// </summary>
    public required FitbitWeightSummaryDto Weight { get; init; }

    /// <summary>
    ///     Gets or sets the calories summary.
    /// </summary>
    public required FitbitCaloriesSummaryDto Calories { get; init; }

    /// <summary>
    ///     Gets or sets the sleep summary.
    /// </summary>
    public required FitbitSleepSummaryDto Sleep { get; init; }

    /// <summary>
    ///     Gets or sets the activity summary.
    /// </summary>
    public required FitbitActivitySummaryDto Activity { get; init; }
}

/// <summary>
///     Weight change summary details.
/// </summary>
public sealed record FitbitWeightSummaryDto
{
    public double? StartingWeightKg { get; init; }

    public double? CurrentWeightKg { get; init; }

    public double? ChangeKg { get; init; }

    public double? BodyFatPercentage { get; init; }

    public double? LeanMassKg { get; init; }
}

/// <summary>
///     Daily calorie summary.
/// </summary>
public sealed record FitbitCaloriesSummaryDto
{
    public int? IntakeCalories { get; init; }

    public int? BurnedCalories { get; init; }

    public int? NetCalories { get; init; }

    public double? CarbsGrams { get; init; }

    public double? FatGrams { get; init; }

    public double? FiberGrams { get; init; }

    public double? ProteinGrams { get; init; }

    public double? SodiumMilligrams { get; init; }
}

/// <summary>
///     Sleep routine summary for the day.
/// </summary>
public sealed record FitbitSleepSummaryDto
{
    public double? TotalSleepHours { get; init; }

    public double? TotalAwakeHours { get; init; }

    public double? EfficiencyPercentage { get; init; }

    public string? Bedtime { get; init; }

    public string? WakeTime { get; init; }

    public FitbitSleepLevelsDto Levels { get; init; } = new();
}

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

/// <summary>
///     Activity summary information.
/// </summary>
public sealed record FitbitActivitySummaryDto(int? Steps, double? DistanceKm, int? Floors);
