namespace Tomeshelf.Application.Contracts.Fitbit;

/// <summary>
///     Activity summary information.
/// </summary>
public sealed record FitbitActivitySummaryDto(int? Steps, double? DistanceKm, int? Floors);