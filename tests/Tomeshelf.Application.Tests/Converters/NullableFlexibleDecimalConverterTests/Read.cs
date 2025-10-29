using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace Tomeshelf.Application.Tests.Converters.NullableFlexibleDecimalConverterTests;

public class NullableFlexibleDecimalConverterReadTests
{
    private readonly JsonSerializerOptions _opts = new(JsonSerializerDefaults.Web) { Converters = { new NullableFlexibleDecimalConverter() } };

    private static string Wrap(object value)
    {
        return JsonSerializer.Serialize(new { v = value });
    }

    [Fact]
    public void Read_Null_ReturnsNull()
    {
        // Arrange
        var json = "{\"v\":null}";
        var doc = JsonDocument.Parse(json);
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        reader.Read();
        reader.Read();
        var converter = new NullableFlexibleDecimalConverter();

        // Act
        var value = converter.Read(ref reader, typeof(decimal?), _opts);
        // Assert
        value.Should()
             .BeNull();
    }

    [Fact]
    public void Read_Number_ReturnsDecimal()
    {
        // Arrange
        var json = "{\"v\": 123.45}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        reader.Read();
        reader.Read();
        var converter = new NullableFlexibleDecimalConverter();

        // Act
        var value = converter.Read(ref reader, typeof(decimal?), _opts);

        // Assert
        value.Should()
             .Be(123.45m);
    }

    [Fact]
    public void Read_StringCurrencyAndCommas_Parses()
    {
        // Arrange
        var json = "{\"v\": \"£1,234.50\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        reader.Read();
        reader.Read();
        var converter = new NullableFlexibleDecimalConverter();

        // Act
        var value = converter.Read(ref reader, typeof(decimal?), _opts);

        // Assert
        value.Should()
             .Be(1234.50m);
    }

    [Fact]
    public void Read_StringEmpty_ReturnsNull()
    {
        // Arrange
        var json = "{\"v\": \"  \"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        reader.Read();
        reader.Read();
        var converter = new NullableFlexibleDecimalConverter();

        // Act
        var value = converter.Read(ref reader, typeof(decimal?), _opts);

        // Assert
        value.Should()
             .BeNull();
    }

    [Fact]
    public void Read_StringInvalid_ReturnsNull()
    {
        // Arrange
        var json = "{\"v\": \"abc\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        reader.Read();
        reader.Read();
        var converter = new NullableFlexibleDecimalConverter();

        // Act
        var value = converter.Read(ref reader, typeof(decimal?), _opts);

        // Assert
        value.Should()
             .BeNull();
    }
}