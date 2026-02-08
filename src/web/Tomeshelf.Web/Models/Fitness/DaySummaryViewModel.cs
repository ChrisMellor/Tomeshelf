using System;

namespace Tomeshelf.Web.Models.Fitness;

public sealed class DaySummaryViewModel
{
    public DateOnly Date { get; init; } = DateOnly.FromDateTime(DateTime.Today);

    public DateTimeOffset GeneratedUtc { get; init; } = DateTimeOffset.UtcNow;

    public FitbitWeightModel Weight { get; init; } = new();

    public FitbitCaloriesModel Calories { get; init; } = new();

    public FitbitSleepModel Sleep { get; init; } = new();

    public FitbitActivityModel Activity { get; init; } = new();
}