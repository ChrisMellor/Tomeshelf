using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.WeightUnitConverterTests;

public class Parse
{
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
        // Act
        var result = WeightUnitConverter.Parse(value);

        // Assert
        result.ShouldBe(expected);
    }
}
