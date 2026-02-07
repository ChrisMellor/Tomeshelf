using Shouldly;

namespace Tomeshelf.Fitbit.Domain.Tests.FitbitDailySnapshotTests;

public class GetNetWeightChangeKg
{
    [Fact]
    public void BothWeightsProvided_ReturnsCorrectDifference()
    {
        var snapshot = new FitbitDailySnapshot
        {
            StartingWeightKg = 70.0,
            CurrentWeightKg = 68.5
        };

        var netChange = snapshot.GetNetWeightChangeKg();

        netChange.HasValue.ShouldBeTrue();
        netChange!.Value.ShouldBeInRange(-1.51, -1.49);
    }

    [Fact]
    public void NoWeightsProvided_ReturnsNull()
    {
        var snapshot = new FitbitDailySnapshot
        {
            StartingWeightKg = null,
            CurrentWeightKg = null
        };

        var netChange = snapshot.GetNetWeightChangeKg();

        netChange.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void OnlyCurrentWeightProvided_ReturnsNull()
    {
        var snapshot = new FitbitDailySnapshot
        {
            StartingWeightKg = null,
            CurrentWeightKg = 68.5
        };

        var netChange = snapshot.GetNetWeightChangeKg();

        netChange.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void OnlyStartingWeightProvided_ReturnsNull()
    {
        var snapshot = new FitbitDailySnapshot
        {
            StartingWeightKg = 70.0,
            CurrentWeightKg = null
        };

        var netChange = snapshot.GetNetWeightChangeKg();

        netChange.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void StartingWeightSameAsCurrent_ReturnsZero()
    {
        var snapshot = new FitbitDailySnapshot
        {
            StartingWeightKg = 75.0,
            CurrentWeightKg = 75.0
        };

        var netChange = snapshot.GetNetWeightChangeKg();

        netChange.HasValue.ShouldBeTrue();
        netChange!.Value.ShouldBeInRange(-0.01, 0.01);
    }
}