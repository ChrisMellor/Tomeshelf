namespace Tomeshelf.Fitbit.Domain.Tests;

public class FitbitDailySnapshotTests
{
    [Fact]
    public void GetNetWeightChangeKg_BothWeightsProvided_ReturnsCorrectDifference()
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
        Assert.True(netChange.HasValue);
        Assert.Equal(-1.5, netChange.Value, 1); // Comparing doubles with tolerance
    }

    [Fact]
    public void GetNetWeightChangeKg_OnlyStartingWeightProvided_ReturnsNull()
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
        Assert.False(netChange.HasValue);
    }

    [Fact]
    public void GetNetWeightChangeKg_OnlyCurrentWeightProvided_ReturnsNull()
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
        Assert.False(netChange.HasValue);
    }

    [Fact]
    public void GetNetWeightChangeKg_NoWeightsProvided_ReturnsNull()
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
        Assert.False(netChange.HasValue);
    }

    [Fact]
    public void GetNetWeightChangeKg_StartingWeightSameAsCurrent_ReturnsZero()
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
        Assert.True(netChange.HasValue);
        Assert.Equal(0.0, netChange.Value, 1);
    }
}
