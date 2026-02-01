using FluentAssertions;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.WeightUnitConverterTests;

public class Convert
{
    [Fact]
    public void WhenNull_ReturnsNull()
    {
        // Act
        var result = WeightUnitConverter.Convert(null, WeightUnit.Kilograms);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void WhenPounds_ReturnsConvertedValue()
    {
        // Act
        var result = WeightUnitConverter.Convert(10, WeightUnit.Pounds);

        // Assert
        result.Should().HaveValue();
        result!.Value.Should().BeApproximately(22.0462, 0.01);
    }
}
