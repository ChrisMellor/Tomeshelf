#nullable enable
namespace Tomeshelf.Web.Models.Fitness;

public sealed class FitbitWeightModel
{
    public double? StartingWeightKg { get; set; }

    public double? CurrentWeightKg { get; set; }

    public double? ChangeKg { get; set; }
}

public sealed class FitbitCaloriesModel
{
    public int? IntakeCalories { get; set; }

    public int? BurnedCalories { get; set; }

    public int? NetCalories { get; set; }
}

public sealed class FitbitSleepModel
{
    public double? TotalSleepHours { get; set; }

    public double? TotalAwakeHours { get; set; }

    public double? EfficiencyPercentage { get; set; }

    public string? Bedtime { get; set; }

    public string? WakeTime { get; set; }
}

public sealed class FitbitActivityModel
{
    public int? Steps { get; set; }

    public double? DistanceKm { get; set; }

    public int? Floors { get; set; }
}