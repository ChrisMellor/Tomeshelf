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

    [Fact]
    public void WhenStones_ReturnsConvertedValue()
    {
        // Act
        var result = WeightUnitConverter.Convert(10, WeightUnit.Stones);

        // Assert
        result.Should().HaveValue();
        result!.Value.Should().BeApproximately(1.5747, 0.01);
    }
}
