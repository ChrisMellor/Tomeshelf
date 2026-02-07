using System.IO;
using System.Text;
using System.Text.Json;
using Shouldly;
using Tomeshelf.Application.Shared;

namespace Tomeshelf.Application.Tests.Converters.NullableFlexibleDecimalConverterTests;

public class Write
{
    [Fact]
    public void Null_WritesNull()
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        var converter = new NullableFlexibleDecimalConverter();

        converter.Write(writer, null, new JsonSerializerOptions());
        writer.Flush();
        var json = Encoding.UTF8.GetString(stream.ToArray());

        json.ShouldBe("null");
    }

    [Fact]
    public void Number_WritesNumber()
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        var converter = new NullableFlexibleDecimalConverter();

        converter.Write(writer, 12.34m, new JsonSerializerOptions());
        writer.Flush();
        var json = Encoding.UTF8.GetString(stream.ToArray());

        json.ShouldBe("12.34");
    }
}