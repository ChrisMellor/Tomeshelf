using Tomeshelf.Infrastructure.Domains.Upload.Records;
using UglyToad.PdfPig;

namespace Tomeshelf.Infrastructure.Domains.Upload.Readers;

internal static class PdfMetadataReader
{
    public static DocumentMetadata GetMetadata(string path)
    {
        using var doc = PdfDocument.Open(path);
        var info = doc.Information;

        return new DocumentMetadata
        {
            Title = string.IsNullOrWhiteSpace(info.Title)
                        ? null
                        : info.Title
        };
    }
}