using Shouldly;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.WeightUnitConverterTests;

public class ToQueryValue
{
    [Theory]
    [InlineData(WeightUnit.Stones, "st")]
    [InlineData(WeightUnit.Pounds, "lb")]
    [InlineData(WeightUnit.Kilograms, "kg")]
    public void ReturnsExpectedToken(WeightUnit unit, string expected)
    {
        // Arrange
        // Act
        var result = WeightUnitConverter.ToQueryValue(unit);

        // Assert
        result.ShouldBe(expected);
    }
}