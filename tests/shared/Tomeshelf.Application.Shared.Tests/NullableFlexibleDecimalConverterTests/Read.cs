using System.Globalization;
using System.Text.Json;
using Tomeshelf.Application.Shared;

namespace Tomeshelf.Application.Shared.Tests.NullableFlexibleDecimalConverterTests;

public class Read
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        Converters = { new NullableFlexibleDecimalConverter() }
    };

    /// <summary>
    ///     Converts the token to decimal.
    /// </summary>
    /// <param name="jsonValue">The json value.</param>
    /// <param name="expected">The expected.</param>
    [Theory]
    [InlineData("123.45", "123.45")]
    [InlineData("\"123.45\"", "123.45")]
    [InlineData("null", null)]
    [InlineData("\"\"", null)]
    public void ConvertsTokenToDecimal(string jsonValue, string? expected)
    {
        // Arrange
        var json = $"{{\"Value\":{jsonValue}}}";

        // Act
        var result = JsonSerializer.Deserialize<TestClass>(json, _options);

        // Assert
        result.ShouldNotBeNull();
        var expectedValue = expected is null ? null : decimal.Parse(expected, CultureInfo.InvariantCulture);
        result.Value.ShouldBe(expectedValue);
    }

    /// <summary>
    ///     Returns the null when token is invalid.
    /// </summary>
    [Fact]
    public void ReturnsNullWhenTokenIsInvalid()
    {
        // Arrange
        var json = "{\"Value\":\"not a decimal\"}";

        // Act
        var result = JsonSerializer.Deserialize<TestClass>(json, _options);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeNull();
    }

    private sealed class TestClass
    {
        public decimal? Value { get; set; }
    }
}
