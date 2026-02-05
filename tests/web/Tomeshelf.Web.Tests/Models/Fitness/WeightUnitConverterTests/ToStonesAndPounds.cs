using FluentAssertions;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.WeightUnitConverterTests;

public class ToStonesAndPounds
{
    [Fact]
    public void ConvertsToStonesAndPounds()
    {
        // Act
        var result = WeightUnitConverter.ToStonesAndPounds(10);

        // Assert
        result.Stones
              .Should()
              .Be(1);
        result.Pounds
              .Should()
              .BeApproximately(8.05, 0.05);
    }
}