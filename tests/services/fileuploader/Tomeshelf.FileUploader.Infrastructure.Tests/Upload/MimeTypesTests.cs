using FluentAssertions;
using Tomeshelf.FileUploader.Infrastructure.Upload;

namespace Tomeshelf.FileUploader.Infrastructure.Tests.Upload;

public class MimeTypesTests
{
    [Theory]
    [InlineData("book.pdf", "application/pdf")]
    [InlineData("book.epub", "application/epub+zip")]
    [InlineData("book.mobi", "application/x-mobipocket-ebook")]
    [InlineData("book.zip", "application/zip")]
    [InlineData("book.cbz", "application/vnd.comicbook+zip")]
    [InlineData("book.cbr", "application/vnd.comicbook-rar")]
    [InlineData("book.unknown", "application/octet-stream")]
    [InlineData("book", "application/octet-stream")]
    public void GetMimeType_ReturnsExpectedResult(string fileName, string expected)
    {
        var mimeType = MimeTypes.GetMimeType(fileName);

        mimeType.Should().Be(expected);
    }
}
