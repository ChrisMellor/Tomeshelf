using Shouldly;

namespace Tomeshelf.Fitbit.Domain.Tests.FitbitDailySnapshotTests;

public class GetNetWeightChangeKg
{
    /// <summary>
    ///     Returns correct difference when both weights are provided.
    /// </summary>
    [Fact]
    public void BothWeightsProvided_ReturnsCorrectDifference()
    {
        // Arrange
        var snapshot = new FitbitDailySnapshot
        {
            StartingWeightKg = 70.0,
            CurrentWeightKg = 68.5
        };

        // Act
        var netChange = snapshot.GetNetWeightChangeKg();

        // Assert
        netChange.HasValue.ShouldBeTrue();
        netChange!.Value.ShouldBeInRange(-1.51, -1.49);
    }

    /// <summary>
    ///     Returns null when no weights are provided.
    /// </summary>
    [Fact]
    public void NoWeightsProvided_ReturnsNull()
    {
        // Arrange
        var snapshot = new FitbitDailySnapshot
        {
            StartingWeightKg = null,
            CurrentWeightKg = null
        };

        // Act
        var netChange = snapshot.GetNetWeightChangeKg();

        // Assert
        netChange.HasValue.ShouldBeFalse();
    }

    /// <summary>
    ///     Returns null when only current weight is provided.
    /// </summary>
    [Fact]
    public void OnlyCurrentWeightProvided_ReturnsNull()
    {
        // Arrange
        var snapshot = new FitbitDailySnapshot
        {
            StartingWeightKg = null,
            CurrentWeightKg = 68.5
        };

        // Act
        var netChange = snapshot.GetNetWeightChangeKg();

        // Assert
        netChange.HasValue.ShouldBeFalse();
    }

    /// <summary>
    ///     Returns null when only starting weight is provided.
    /// </summary>
    [Fact]
    public void OnlyStartingWeightProvided_ReturnsNull()
    {
        // Arrange
        var snapshot = new FitbitDailySnapshot
        {
            StartingWeightKg = 70.0,
            CurrentWeightKg = null
        };

        // Act
        var netChange = snapshot.GetNetWeightChangeKg();

        // Assert
        netChange.HasValue.ShouldBeFalse();
    }

    /// <summary>
    ///     Returns zero when the starting weight same as current.
    /// </summary>
    [Fact]
    public void StartingWeightSameAsCurrent_ReturnsZero()
    {
        // Arrange
        var snapshot = new FitbitDailySnapshot
        {
            StartingWeightKg = 75.0,
            CurrentWeightKg = 75.0
        };

        // Act
        var netChange = snapshot.GetNetWeightChangeKg();

        // Assert
        netChange.HasValue.ShouldBeTrue();
        netChange!.Value.ShouldBeInRange(-0.01, 0.01);
    }
}