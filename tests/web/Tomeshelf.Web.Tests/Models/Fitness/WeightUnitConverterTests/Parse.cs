using Shouldly;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.WeightUnitConverterTests;

public class Parse
{
    /// <summary>
    ///     Returns the expected unit.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="expected">The expected.</param>
    [Theory]
    [InlineData(null, WeightUnit.Stones)]
    [InlineData("", WeightUnit.Stones)]
    [InlineData("  ", WeightUnit.Stones)]
    [InlineData("lb", WeightUnit.Pounds)]
    [InlineData("pounds", WeightUnit.Pounds)]
    [InlineData("kg", WeightUnit.Kilograms)]
    [InlineData("kilograms", WeightUnit.Kilograms)]
    [InlineData("unknown", WeightUnit.Stones)]
    public void ReturnsExpectedUnit(string value, WeightUnit expected)
    {
        // Arrange
        // Act
        var result = WeightUnitConverter.Parse(value);

        // Assert
        result.ShouldBe(expected);
    }
}