using System;

namespace Tomeshelf.Web.Models.Fitness;

public sealed class FitbitOverviewDayModel
{
    public string Date { get; set; } = DateOnly.FromDateTime(DateTime.Today)
                                               .ToString("yyyy-MM-dd");

    public double? WeightKg { get; set; }

    public int? Steps { get; set; }

    public double? SleepHours { get; set; }

    public int? NetCalories { get; set; }
}