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

    public WeightUnit Unit { get; init; } = WeightUnit.Stones;

    public DaySummaryViewModel Summary { get; init; }

    public FitnessRangeViewModel Last7Days { get; init; }

    public FitnessRangeViewModel Last30Days { get; init; }

    public string ErrorMessage { get; init; }

    public bool HasData => ErrorMessage is null && Summary is not null;

    /// <summary>
    ///     Emptys.
    /// </summary>
    /// <param name="selectedDate">The selected date.</param>
    /// <param name="unit">The unit.</param>
    /// <param name="message">The message.</param>
    /// <returns>The result of the operation.</returns>
    public static FitnessDashboardViewModel Empty(string selectedDate, WeightUnit unit, string message = null)
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
            Unit = unit,
            ErrorMessage = message
        };
    }
}