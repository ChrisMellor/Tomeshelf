using System.Text;
using System.Text.Json;
using Shouldly;
using Tomeshelf.Application.Shared;

namespace Tomeshelf.Application.Tests.Converters.NullableFlexibleDecimalConverterTests;

public class Read
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { Converters = { new NullableFlexibleDecimalConverter() } };

    [Fact]
    public void Null_ReturnsNull()
    {
        var json = "{\"v\":null}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        reader.Read();
        reader.Read();
        var converter = new NullableFlexibleDecimalConverter();

        var value = converter.Read(ref reader, typeof(decimal?), _options);

        value.ShouldBeNull();
    }

    [Fact]
    public void Number_ReturnsDecimal()
    {
        var json = "{\"v\": 123.45}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        reader.Read();
        reader.Read();
        var converter = new NullableFlexibleDecimalConverter();

        var value = converter.Read(ref reader, typeof(decimal?), _options);

        value.ShouldBe(123.45m);
    }

    [Fact]
    public void StringCurrencyAndCommas_Parses()
    {
        var json = "{\"v\": \"$1,234.50\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        reader.Read();
        reader.Read();
        var converter = new NullableFlexibleDecimalConverter();

        var value = converter.Read(ref reader, typeof(decimal?), _options);

        value.ShouldBe(1234.50m);
    }

    [Fact]
    public void StringEmpty_ReturnsNull()
    {
        var json = "{\"v\": \"  \"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        reader.Read();
        reader.Read();
        var converter = new NullableFlexibleDecimalConverter();

        var value = converter.Read(ref reader, typeof(decimal?), _options);

        value.ShouldBeNull();
    }

    [Fact]
    public void StringInvalid_ReturnsNull()
    {
        var json = "{\"v\": \"abc\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        reader.Read();
        reader.Read();
        var converter = new NullableFlexibleDecimalConverter();

        var value = converter.Read(ref reader, typeof(decimal?), _options);

        value.ShouldBeNull();
    }
}