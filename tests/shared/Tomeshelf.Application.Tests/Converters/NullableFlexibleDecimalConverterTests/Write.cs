using System.IO;
using System.Text;
using System.Text.Json;
using Shouldly;
using Tomeshelf.Application.Shared;

namespace Tomeshelf.Application.Tests.Converters.NullableFlexibleDecimalConverterTests;

public class Write
{
    /// <summary>
    ///     Writes null when the value is null.
    /// </summary>
    [Fact]
    public void Null_WritesNull()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        var converter = new NullableFlexibleDecimalConverter();

        // Act
        converter.Write(writer, null, new JsonSerializerOptions());
        writer.Flush();
        var json = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        json.ShouldBe("null");
    }

    /// <summary>
    ///     Writes number when the value is a number.
    /// </summary>
    [Fact]
    public void Number_WritesNumber()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        var converter = new NullableFlexibleDecimalConverter();

        // Act
        converter.Write(writer, 12.34m, new JsonSerializerOptions());
        writer.Flush();
        var json = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        json.ShouldBe("12.34");
    }
}