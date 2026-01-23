using System;

namespace Tomeshelf.Application.Shared.Contracts.Fitbit;

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