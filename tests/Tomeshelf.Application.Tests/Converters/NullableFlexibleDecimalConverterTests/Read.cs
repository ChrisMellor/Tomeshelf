using FluentAssertions;
using System.Text.Json;
using Tomeshelf.Application;

namespace Tomeshelf.Application.Tests.Converters.NullableFlexibleDecimalConverterTests;

public class NullableFlexibleDecimalConverter_Read_Tests
{
    private readonly JsonSerializerOptions _opts = new(JsonSerializerDefaults.Web)
    {
        Converters = { new NullableFlexibleDecimalConverter() }
    };

    private static string Wrap(object value) => JsonSerializer.Serialize(new { v = value });

    [Fact]
    public void Read_Null_ReturnsNull()
    {
        var json = "{\"v\":null}";
        var doc = JsonDocument.Parse(json);
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read(); // StartObject
        reader.Read(); // PropertyName v
        reader.Read(); // Null
        var converter = new NullableFlexibleDecimalConverter();
        decimal? value = converter.Read(ref reader, typeof(decimal?), _opts);
        value.Should().BeNull();
    }

    [Fact]
    public void Read_Number_ReturnsDecimal()
    {
        var json = "{\"v\": 123.45}";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read(); reader.Read(); reader.Read();
        var converter = new NullableFlexibleDecimalConverter();
        var value = converter.Read(ref reader, typeof(decimal?), _opts);
        value.Should().Be(123.45m);
    }

    [Fact]
    public void Read_StringCurrencyAndCommas_Parses()
    {
        var json = "{\"v\": \"£1,234.50\"}";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read(); reader.Read(); reader.Read();
        var converter = new NullableFlexibleDecimalConverter();
        var value = converter.Read(ref reader, typeof(decimal?), _opts);
        value.Should().Be(1234.50m);
    }

    [Fact]
    public void Read_StringEmpty_ReturnsNull()
    {
        var json = "{\"v\": \"  \"}";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read(); reader.Read(); reader.Read();
        var converter = new NullableFlexibleDecimalConverter();
        var value = converter.Read(ref reader, typeof(decimal?), _opts);
        value.Should().BeNull();
    }

    [Fact]
    public void Read_StringInvalid_ReturnsNull()
    {
        var json = "{\"v\": \"abc\"}";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read(); reader.Read(); reader.Read();
        var converter = new NullableFlexibleDecimalConverter();
        var value = converter.Read(ref reader, typeof(decimal?), _opts);
        value.Should().BeNull();
    }
}

