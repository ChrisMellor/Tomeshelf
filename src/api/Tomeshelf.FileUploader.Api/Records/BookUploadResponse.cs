using Tomeshelf.Infrastructure.Bundles.Upload;

namespace Tomeshelf.FileUploader.Api.Records;

public sealed record BookUploadResponse
{
    public BookUploadResponse(string bundleName, string bookTitle, int filesUploaded, int filesSkipped)
    {
        BundleName = bundleName;
        BookTitle = bookTitle;
        FilesUploaded = filesUploaded;
        FilesSkipped = filesSkipped;
    }

    public string BundleName { get; init; }

    public string BookTitle { get; init; }

    public int FilesUploaded { get; init; }

    public int FilesSkipped { get; init; }

    public static BookUploadResponse FromResult(BookUploadResult result)
    {
        return new BookUploadResponse(result.BundleName, result.BookTitle, result.FilesUploaded, result.FilesSkipped);
    }

    public void Deconstruct(out string bundleName, out string bookTitle, out int filesUploaded, out int filesSkipped)
    {
        bundleName = BundleName;
        bookTitle = BookTitle;
        filesUploaded = FilesUploaded;
        filesSkipped = FilesSkipped;
    }
}