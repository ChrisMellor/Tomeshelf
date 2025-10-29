using System;

namespace Tomeshelf.Web.Models.Fitness;

public sealed class FitbitDashboardModel
{
    public string Date { get; set; } = DateOnly.FromDateTime(DateTime.Today)
                                               .ToString("yyyy-MM-dd");

    public DateTimeOffset GeneratedUtc { get; set; } = DateTimeOffset.UtcNow;

    public FitbitWeightModel Weight { get; set; } = new FitbitWeightModel();

    public FitbitCaloriesModel Calories { get; set; } = new FitbitCaloriesModel();

    public FitbitSleepModel Sleep { get; set; } = new FitbitSleepModel();

    public FitbitActivityModel Activity { get; set; } = new FitbitActivityModel();
}