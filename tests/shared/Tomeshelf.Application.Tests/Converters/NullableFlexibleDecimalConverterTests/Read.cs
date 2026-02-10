using System.Text;
using System.Text.Json;
using Shouldly;
using Tomeshelf.Application.Shared;

namespace Tomeshelf.Application.Tests.Converters.NullableFlexibleDecimalConverterTests;

public class Read
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { Converters = { new NullableFlexibleDecimalConverter() } };

    /// <summary>
    ///     Returns null when the value is null.
    /// </summary>
    [Fact]
    public void Null_ReturnsNull()
    {
        // Arrange
        var json = "{\"v\":null}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        reader.Read();
        reader.Read();
        var converter = new NullableFlexibleDecimalConverter();

        // Act
        var value = converter.Read(ref reader, typeof(decimal?), _options);

        // Assert
        value.ShouldBeNull();
    }

    /// <summary>
    ///     Returns decimal when the value is a number.
    /// </summary>
    [Fact]
    public void Number_ReturnsDecimal()
    {
        // Arrange
        var json = "{\"v\": 123.45}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        reader.Read();
        reader.Read();
        var converter = new NullableFlexibleDecimalConverter();

        // Act
        var value = converter.Read(ref reader, typeof(decimal?), _options);

        // Assert
        value.ShouldBe(123.45m);
    }

    /// <summary>
    ///     Parses when the string contains currency and commas.
    /// </summary>
    [Fact]
    public void StringCurrencyAndCommas_Parses()
    {
        // Arrange
        var json = "{\"v\": \"$1,234.50\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        reader.Read();
        reader.Read();
        var converter = new NullableFlexibleDecimalConverter();

        // Act
        var value = converter.Read(ref reader, typeof(decimal?), _options);

        // Assert
        value.ShouldBe(1234.50m);
    }

    /// <summary>
    ///     Returns null when the string is empty.
    /// </summary>
    [Fact]
    public void StringEmpty_ReturnsNull()
    {
        // Arrange
        var json = "{\"v\": \"  \"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        reader.Read();
        reader.Read();
        var converter = new NullableFlexibleDecimalConverter();

        // Act
        var value = converter.Read(ref reader, typeof(decimal?), _options);

        // Assert
        value.ShouldBeNull();
    }

    /// <summary>
    ///     Returns null when the string is invalid.
    /// </summary>
    [Fact]
    public void StringInvalid_ReturnsNull()
    {
        // Arrange
        var json = "{\"v\": \"abc\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        reader.Read();
        reader.Read();
        var converter = new NullableFlexibleDecimalConverter();

        // Act
        var value = converter.Read(ref reader, typeof(decimal?), _options);

        // Assert
        value.ShouldBeNull();
    }
}