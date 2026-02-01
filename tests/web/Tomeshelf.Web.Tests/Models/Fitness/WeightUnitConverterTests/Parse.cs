using FluentAssertions;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.WeightUnitConverterTests;

public class Parse
{
    [Theory]
    [InlineData(null, WeightUnit.Stones)]
    [InlineData("", WeightUnit.Stones)]
    [InlineData(" ", WeightUnit.Stones)]
    [InlineData("kg", WeightUnit.Kilograms)]
    [InlineData("LBs", WeightUnit.Pounds)]
    [InlineData("stone", WeightUnit.Stones)]
    [InlineData("unknown", WeightUnit.Stones)]
    public void HandlesInput(string? value, WeightUnit expected)
    {
        // Act
        var unit = WeightUnitConverter.Parse(value);

        // Assert
        unit.Should().Be(expected);
    }
}
