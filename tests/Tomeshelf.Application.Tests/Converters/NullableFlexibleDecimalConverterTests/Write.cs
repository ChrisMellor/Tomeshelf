using FluentAssertions;
using System.IO;
using System.Text.Json;
using Tomeshelf.Application;

namespace Tomeshelf.Application.Tests.Converters.NullableFlexibleDecimalConverterTests;

public class NullableFlexibleDecimalConverter_Write_Tests
{
    [Fact]
    public void Write_Null_WritesNull()
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        var converter = new NullableFlexibleDecimalConverter();
        converter.Write(writer, null, new JsonSerializerOptions());
        writer.Flush();
        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        json.Should().Be("null");
    }

    [Fact]
    public void Write_Number_WritesNumber()
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        var converter = new NullableFlexibleDecimalConverter();
        converter.Write(writer, 12.34m, new JsonSerializerOptions());
        writer.Flush();
        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        json.Should().Be("12.34");
    }
}

