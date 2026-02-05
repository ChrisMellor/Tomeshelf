using FluentAssertions;

namespace Tomeshelf.Fitbit.Domain.Tests.FitbitDailySnapshotTests;

public class GetNetWeightChangeKg
{
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
        netChange.Should()
                 .HaveValue();
        netChange!.Value
                  .Should()
                  .BeApproximately(-1.5, 0.01);
    }

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
        netChange.Should()
                 .NotHaveValue();
    }

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
        netChange.Should()
                 .NotHaveValue();
    }

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
        netChange.Should()
                 .NotHaveValue();
    }

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
        netChange.Should()
                 .HaveValue();
        netChange!.Value
                  .Should()
                  .BeApproximately(0.0, 0.01);
    }
}