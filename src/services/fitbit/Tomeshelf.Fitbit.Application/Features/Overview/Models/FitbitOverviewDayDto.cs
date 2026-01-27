namespace Tomeshelf.Fitbit.Application.Features.Overview.Models;

public sealed record FitbitOverviewDayDto
{
    public required string Date { get; init; }

    public double? WeightKg { get; init; }

    public int? Steps { get; init; }

    public double? SleepHours { get; init; }

    public int? NetCalories { get; init; }
}
