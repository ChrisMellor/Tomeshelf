using System.IO;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Tomeshelf.Application.Shared;

namespace Tomeshelf.Application.Tests.Converters.NullableFlexibleDecimalConverterTests;

public class Write
{
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
        json.Should()
            .Be("null");
    }

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
        json.Should()
            .Be("12.34");
    }
}