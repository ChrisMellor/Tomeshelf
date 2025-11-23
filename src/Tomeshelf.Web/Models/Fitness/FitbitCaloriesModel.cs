namespace Tomeshelf.Web.Models.Fitness;

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