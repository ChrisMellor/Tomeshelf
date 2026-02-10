using System.Globalization;
using System.Text.Json;
using Tomeshelf.Application.Shared;

namespace Tomeshelf.Application.Shared.Tests.NullableFlexibleDecimalConverterTests;

public class Write
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        Converters = { new NullableFlexibleDecimalConverter() }
    };

    /// <summary>
    ///     Converts the decimal to string.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="expectedJsonValue">The expected json value.</param>
    [Theory]
    [InlineData("123.45", "123.45")]
    [InlineData(null, "null")]
    public void ConvertsDecimalToString(string? value, string expectedJsonValue)
    {
        // Arrange
        var testObject = new TestClass
        {
            Value = value is null ? null : decimal.Parse(value, CultureInfo.InvariantCulture)
        };

        // Act
        var json = JsonSerializer.Serialize(testObject, _options);

        // Assert
        json.ShouldBe($"{{\"Value\":{expectedJsonValue}}}");
    }

    private sealed class TestClass
    {
        public decimal? Value { get; set; }
    }
}
