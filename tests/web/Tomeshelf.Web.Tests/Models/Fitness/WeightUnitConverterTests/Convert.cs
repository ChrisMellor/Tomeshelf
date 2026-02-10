using Shouldly;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.WeightUnitConverterTests;

public class Convert
{
    /// <summary>
    ///     Returns null when the value is null.
    /// </summary>
    [Fact]
    public void WhenNull_ReturnsNull()
    {
        // Arrange
        // Act
        var result = WeightUnitConverter.Convert(null, WeightUnit.Kilograms);

        // Assert
        result.ShouldBeNull();
    }

    /// <summary>
    ///     Returns converted value when the value is a pounds.
    /// </summary>
    [Fact]
    public void WhenPounds_ReturnsConvertedValue()
    {
        // Arrange
        // Act
        var result = WeightUnitConverter.Convert(10, WeightUnit.Pounds);

        // Assert
        result.HasValue.ShouldBeTrue();
        result!.Value.ShouldBeInRange(22.0362, 22.0562);
    }

    /// <summary>
    ///     Returns converted value when the value is a stones.
    /// </summary>
    [Fact]
    public void WhenStones_ReturnsConvertedValue()
    {
        // Arrange
        // Act
        var result = WeightUnitConverter.Convert(10, WeightUnit.Stones);

        // Assert
        result.HasValue.ShouldBeTrue();
        result!.Value.ShouldBeInRange(1.5647, 1.5847);
    }
}