using System.Globalization;
using System.IO;

namespace Tomeshelf.HumbleBundle.Infrastructure.Bundles.Upload;

internal static class MimeTypes
{
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