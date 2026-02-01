using System;
using Tomeshelf.Web.Models.Fitness;
using Xunit;

namespace Tomeshelf.Web.Tests.Models.Fitness;

public class WeightUnitConverterTests
{
    [Theory]
    [InlineData(null, WeightUnit.Stones)]
    [InlineData("", WeightUnit.Stones)]
    [InlineData(" ", WeightUnit.Stones)]
    [InlineData("kg", WeightUnit.Kilograms)]
    [InlineData("LBs", WeightUnit.Pounds)]
    [InlineData("stone", WeightUnit.Stones)]
    [InlineData("unknown", WeightUnit.Stones)]
    public void Parse_HandlesInput(string? value, WeightUnit expected)
    {
        var unit = WeightUnitConverter.Parse(value);

        Assert.Equal(expected, unit);
    }

    [Theory]
    [InlineData(WeightUnit.Stones, "st")]
    [InlineData(WeightUnit.Pounds, "lb")]
    [InlineData(WeightUnit.Kilograms, "kg")]
    public void ToQueryValue_ReturnsExpectedString(WeightUnit unit, string expected)
    {
        Assert.Equal(expected, WeightUnitConverter.ToQueryValue(unit));
    }

    [Fact]
    public void Convert_WhenNull_ReturnsNull()
    {
        Assert.Null(WeightUnitConverter.Convert(null, WeightUnit.Kilograms));
    }

    [Fact]
    public void Convert_WhenPounds_ReturnsConvertedValue()
    {
        var result = WeightUnitConverter.Convert(10, WeightUnit.Pounds);

        Assert.True(result.HasValue);
        Assert.InRange(result.Value, 22.04, 22.05);
    }

    [Fact]
    public void ToStonesAndPounds_ConvertsCorrectly()
    {
        var (stones, pounds) = WeightUnitConverter.ToStonesAndPounds(70);

        Assert.True(stones > 0);
        Assert.InRange(pounds, 0, 14);
    }
}
