namespace Tomeshelf.Web.Models.Fitness;

public sealed class FitbitWeightModel
{
    public double? StartingWeightKg { get; set; }

    public double? CurrentWeightKg { get; set; }

    public double? ChangeKg { get; set; }

    public double? BodyFatPercentage { get; set; }

    public double? LeanMassKg { get; set; }
}

public sealed class FitbitCaloriesModel
{
    public int? IntakeCalories { get; set; }

    public int? BurnedCalories { get; set; }

    public int? NetCalories { get; set; }

    public double? CarbsGrams { get; set; }

    public double? FatGrams { get; set; }

    public double? FiberGrams { get; set; }

    public double? ProteinGrams { get; set; }

    public double? SodiumMilligrams { get; set; }
}

public sealed class FitbitSleepModel
{
    public double? TotalSleepHours { get; set; }

    public double? TotalAwakeHours { get; set; }

    public double? EfficiencyPercentage { get; set; }

    public string Bedtime { get; set; }

    public string WakeTime { get; set; }

    public FitbitSleepLevelsModel Levels { get; set; } = new();
}

public sealed class FitbitActivityModel
{
    public int? Steps { get; set; }

    public double? DistanceKm { get; set; }

    public int? Floors { get; set; }
}

public sealed class FitbitSleepLevelsModel
{
    public int? DeepMinutes { get; set; }

    public int? LightMinutes { get; set; }

    public int? RemMinutes { get; set; }

    public int? WakeMinutes { get; set; }
}