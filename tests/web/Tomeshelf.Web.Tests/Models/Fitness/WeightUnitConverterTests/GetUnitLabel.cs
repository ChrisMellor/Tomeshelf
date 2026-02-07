using Shouldly;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.WeightUnitConverterTests;

public class GetUnitLabel
{
    [Theory]
    [InlineData(WeightUnit.Stones, "st")]
    [InlineData(WeightUnit.Pounds, "lb")]
    [InlineData(WeightUnit.Kilograms, "kg")]
    public void ReturnsExpectedLabel(WeightUnit unit, string expected)
    {
        var result = WeightUnitConverter.GetUnitLabel(unit);

        result.ShouldBe(expected);
    }
}