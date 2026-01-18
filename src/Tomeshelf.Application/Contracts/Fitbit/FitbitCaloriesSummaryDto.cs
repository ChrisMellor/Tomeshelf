namespace Tomeshelf.Application.Contracts.Fitbit;

/// <summary>
///     Daily calorie summary.
/// </summary>
public sealed record FitbitCaloriesSummaryDto
{
    public int? IntakeCalories { get; init; }

    public int? BurnedCalories { get; init; }

    public int? NetCalories { get; init; }

    public double? CarbsGrams { get; init; }

    public double? FatGrams { get; init; }

    public double? FiberGrams { get; init; }

    public double? ProteinGrams { get; init; }

    public double? SodiumMilligrams { get; init; }
}