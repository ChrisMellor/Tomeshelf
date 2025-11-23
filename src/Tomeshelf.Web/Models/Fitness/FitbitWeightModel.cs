namespace Tomeshelf.Web.Models.Fitness;

public sealed class FitbitWeightModel
{
    public double? StartingWeightKg { get; set; }

    public double? CurrentWeightKg { get; set; }

    public double? ChangeKg { get; set; }

    public double? BodyFatPercentage { get; set; }

    public double? LeanMassKg { get; set; }
}