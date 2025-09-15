using FluentAssertions;
using System.IO;
using System.Text.Json;

namespace Tomeshelf.Application.Tests.Converters.NullableFlexibleDecimalConverterTests;

public class NullableFlexibleDecimalConverterWriteTests
{
    [Fact]
    public void Write_Null_WritesNull()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        var converter = new NullableFlexibleDecimalConverter();

        // Act
        converter.Write(writer, null, new JsonSerializerOptions());
        writer.Flush();
        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        json.Should().Be("null");
    }

    [Fact]
    public void Write_Number_WritesNumber()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        var converter = new NullableFlexibleDecimalConverter();

        // Act
        converter.Write(writer, 12.34m, new JsonSerializerOptions());
        writer.Flush();
        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        json.Should().Be("12.34");
    }
}
