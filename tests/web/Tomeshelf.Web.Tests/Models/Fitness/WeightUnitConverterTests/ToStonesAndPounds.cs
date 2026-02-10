using Shouldly;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.WeightUnitConverterTests;

public class ToStonesAndPounds
{
    /// <summary>
    ///     Converts the to stones and pounds.
    /// </summary>
    [Fact]
    public void ConvertsToStonesAndPounds()
    {
        // Arrange
        // Act
        var result = WeightUnitConverter.ToStonesAndPounds(10);

        // Assert
        result.Stones.ShouldBe(1);
        result.Pounds.ShouldBeInRange(8.0, 8.1);
    }
}