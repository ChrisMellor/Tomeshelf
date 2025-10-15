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
public sealed record FitbitWeightSummaryDto(double? StartingWeightKg, double? CurrentWeightKg, double? ChangeKg);

/// <summary>
///     Daily calorie summary.
/// </summary>
public sealed record FitbitCaloriesSummaryDto(int? IntakeCalories, int? BurnedCalories, int? NetCalories);

/// <summary>
///     Sleep routine summary for the day.
/// </summary>
public sealed record FitbitSleepSummaryDto(double? TotalSleepHours, double? TotalAwakeHours, double? EfficiencyPercentage, string? Bedtime, string? WakeTime);

/// <summary>
///     Activity summary information.
/// </summary>
public sealed record FitbitActivitySummaryDto(int? Steps, double? DistanceKm, int? Floors);