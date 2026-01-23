namespace Tomeshelf.Application.Shared.Contracts.Fitbit;

/// <summary>
///     Weight change summary details.
/// </summary>
public sealed record FitbitWeightSummaryDto
{
    public double? StartingWeightKg { get; init; }

    public double? CurrentWeightKg { get; init; }

    public double? ChangeKg { get; init; }

    public double? BodyFatPercentage { get; init; }

    public double? LeanMassKg { get; init; }
}