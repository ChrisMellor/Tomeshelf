using System;

namespace Tomeshelf.Fitbit.Domain;

/// <summary>
///     Persisted daily snapshot of aggregated Fitbit metrics.
/// </summary>
public sealed class FitbitDailySnapshot
{
    /// <summary>
    ///     Gets or sets the local date the snapshot represents.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    ///     Gets or sets when the snapshot was generated (UTC).
    /// </summary>
    public DateTimeOffset GeneratedUtc { get; set; }

    public double? StartingWeightKg { get; set; }

    public double? CurrentWeightKg { get; set; }

    public double? ChangeWeightKg { get; set; }

    public double? BodyFatPercentage { get; set; }

    public double? LeanMassKg { get; set; }

    public int? IntakeCalories { get; set; }

    public int? BurnedCalories { get; set; }

    public int? NetCalories { get; set; }

    public double? CarbsGrams { get; set; }

    public double? FatGrams { get; set; }

    public double? FiberGrams { get; set; }

    public double? ProteinGrams { get; set; }

    public double? SodiumMilligrams { get; set; }

    public double? TotalSleepHours { get; set; }

    public double? TotalAwakeHours { get; set; }

    public double? SleepEfficiencyPercentage { get; set; }

    public string Bedtime { get; set; }

    public string WakeTime { get; set; }

    public int? SleepDeepMinutes { get; set; }

    public int? SleepLightMinutes { get; set; }

    public int? SleepRemMinutes { get; set; }

    public int? SleepWakeMinutes { get; set; }

    public int? Steps { get; set; }

    public double? DistanceKm { get; set; }

    public int? Floors { get; set; }

    /// <summary>
    ///     Calculates the net weight change in kilograms.
    /// </summary>
    /// <returns>The net weight change in kilograms, or null if starting or current weight is not available.</returns>
    public double? GetNetWeightChangeKg()
    {
        if (StartingWeightKg.HasValue && CurrentWeightKg.HasValue)
        {
            return CurrentWeightKg.Value - StartingWeightKg.Value;
        }

        return null;
    }
}