using Shouldly;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.WeightUnitConverterTests;

public class GetUnitLabel
{
    /// <summary>
    ///     Returns the expected label.
    /// </summary>
    /// <param name="unit">The unit.</param>
    /// <param name="expected">The expected.</param>
    [Theory]
    [InlineData(WeightUnit.Stones, "st")]
    [InlineData(WeightUnit.Pounds, "lb")]
    [InlineData(WeightUnit.Kilograms, "kg")]
    public void ReturnsExpectedLabel(WeightUnit unit, string expected)
    {
        // Arrange
        // Act
        var result = WeightUnitConverter.GetUnitLabel(unit);

        // Assert
        result.ShouldBe(expected);
    }
}