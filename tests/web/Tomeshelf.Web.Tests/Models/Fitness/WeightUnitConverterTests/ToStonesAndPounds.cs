using FluentAssertions;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.WeightUnitConverterTests;

public class ToStonesAndPounds
{
    [Fact]
    public void ConvertsCorrectly()
    {
        // Act
        var (stones, pounds) = WeightUnitConverter.ToStonesAndPounds(70);

        // Assert
        stones.Should().BeGreaterThan(0);
        pounds.Should().BeInRange(0, 14);
    }
}
