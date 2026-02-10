using UglyToad.PdfPig;

namespace Tomeshelf.FileUploader.Infrastructure.Upload;

internal static class PdfMetadataReader
{
    /// <summary>
    ///     Gets the metadata.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The result of the operation.</returns>
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