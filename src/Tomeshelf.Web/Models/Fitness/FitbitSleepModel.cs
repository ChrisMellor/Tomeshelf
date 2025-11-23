namespace Tomeshelf.Web.Models.Fitness;

public sealed class FitbitSleepModel
{
    public double? TotalSleepHours { get; set; }

    public double? TotalAwakeHours { get; set; }

    public double? EfficiencyPercentage { get; set; }

    public string Bedtime { get; set; }

    public string WakeTime { get; set; }

    public FitbitSleepLevelsModel Levels { get; set; } = new();
}