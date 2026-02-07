using Shouldly;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.WeightUnitConverterTests;

public class Convert
{
    [Fact]
    public void WhenNull_ReturnsNull()
    {
        var result = WeightUnitConverter.Convert(null, WeightUnit.Kilograms);

        result.ShouldBeNull();
    }

    [Fact]
    public void WhenPounds_ReturnsConvertedValue()
    {
        var result = WeightUnitConverter.Convert(10, WeightUnit.Pounds);

        result.HasValue.ShouldBeTrue();
        result!.Value.ShouldBeInRange(22.0362, 22.0562);
    }

    [Fact]
    public void WhenStones_ReturnsConvertedValue()
    {
        var result = WeightUnitConverter.Convert(10, WeightUnit.Stones);

        result.HasValue.ShouldBeTrue();
        result!.Value.ShouldBeInRange(1.5647, 1.5847);
    }
}