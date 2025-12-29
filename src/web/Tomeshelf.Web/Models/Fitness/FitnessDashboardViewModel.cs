using System;

namespace Tomeshelf.Web.Models.Fitness;

public sealed class FitnessDashboardViewModel
{
    public string SelectedDate { get; init; } = DateOnly.FromDateTime(DateTime.Today)
                                                        .ToString("yyyy-MM-dd");

    public string TodayIso { get; init; } = DateOnly.FromDateTime(DateTime.Today)
                                                    .ToString("yyyy-MM-dd");

    public string PreviousDate { get; init; } = DateOnly.FromDateTime(DateTime.Today)
                                                        .AddDays(-1)
                                                        .ToString("yyyy-MM-dd");

    public string NextDate { get; init; }

    public bool IsToday => string.Equals(SelectedDate, TodayIso, StringComparison.Ordinal);

    public bool CanRefresh => !IsToday;

    public DaySummaryViewModel Summary { get; init; }

    public string ErrorMessage { get; init; }

    public bool HasData => ErrorMessage is null && Summary is not null;

    public static FitnessDashboardViewModel Empty(string selectedDate, string message = null)
    {
        var selected = DateOnly.Parse(selectedDate);
        var today = DateOnly.FromDateTime(DateTime.Today);

        return new FitnessDashboardViewModel
        {
            SelectedDate = selectedDate,
            TodayIso = today.ToString("yyyy-MM-dd"),
            PreviousDate = selected.AddDays(-1)
                                   .ToString("yyyy-MM-dd"),
            NextDate = selected < today
                ? selected.AddDays(1)
                          .ToString("yyyy-MM-dd")
                : null,
            ErrorMessage = message
        };
    }
}

public sealed class DaySummaryViewModel
{
    public DateOnly Date { get; init; } = DateOnly.FromDateTime(DateTime.Today);

    public DateTimeOffset GeneratedUtc { get; init; } = DateTimeOffset.UtcNow;

    public FitbitWeightModel Weight { get; init; } = new();

    public FitbitCaloriesModel Calories { get; init; } = new();

    public FitbitSleepModel Sleep { get; init; } = new();

    public FitbitActivityModel Activity { get; init; } = new();
}