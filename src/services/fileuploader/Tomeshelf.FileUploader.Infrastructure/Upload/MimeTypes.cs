using System.Globalization;
using System.IO;

namespace Tomeshelf.FileUploader.Infrastructure.Upload;

internal static class MimeTypes
{
    /// <summary>
    ///     Gets the mime type.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns>The resulting string.</returns>
    public static string GetMimeType(string fileName)
    {
        var ext = Path.GetExtension(fileName)
                     ?.ToLower(CultureInfo.InvariantCulture);

        return ext switch
        {
            ".pdf" => "application/pdf",
            ".epub" => "application/epub+zip",
            ".mobi" => "application/x-mobipocket-ebook",
            ".zip" => "application/zip",
            ".cbz" => "application/vnd.comicbook+zip",
            ".cbr" => "application/vnd.comicbook-rar",
            _ => "application/octet-stream"
        };
    }
}