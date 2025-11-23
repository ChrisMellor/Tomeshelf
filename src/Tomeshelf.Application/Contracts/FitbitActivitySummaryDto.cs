namespace Tomeshelf.Application.Contracts;

/// <summary>
///     Activity summary information.
/// </summary>
public sealed record FitbitActivitySummaryDto
{
    /// <summary>
    ///     Activity summary information.
    /// </summary>
    public FitbitActivitySummaryDto(int? steps, double? distanceKm, int? floors)
    {
        Steps = steps;
        DistanceKm = distanceKm;
        Floors = floors;
    }

    public int? Steps { get; init; }

    public double? DistanceKm { get; init; }

    public int? Floors { get; init; }

    public void Deconstruct(out int? steps, out double? distanceKm, out int? floors)
    {
        steps = Steps;
        distanceKm = DistanceKm;
        floors = Floors;
    }
}