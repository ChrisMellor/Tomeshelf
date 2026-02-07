using System.IO.Compression;
using Shouldly;
using Tomeshelf.FileUploader.Infrastructure.Upload;

namespace Tomeshelf.FileUploader.Infrastructure.Tests.Upload.EpubMetadataReaderTests;

public class GetMetadata
{
    [Fact]
    public void ReadsTitleFromEpub()
    {
        var tempDir = CreateTempDirectory();
        var epubPath = Path.Combine(tempDir, "book.epub");

        try
        {
            using (var zip = ZipFile.Open(epubPath, ZipArchiveMode.Create))
            {
                var containerEntry = zip.CreateEntry("META-INF/container.xml");
                using (var writer = new StreamWriter(containerEntry.Open()))
                {
                    writer.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + "<container version=\"1.0\" xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\">\n" + "  <rootfiles>\n" + "    <rootfile full-path=\"content.opf\" media-type=\"application/oebps-package+xml\"/>\n" + "  </rootfiles>\n" + "</container>");
                }

                var opfEntry = zip.CreateEntry("content.opf");
                using (var writer = new StreamWriter(opfEntry.Open()))
                {
                    writer.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + "<package xmlns=\"http://www.idpf.org/2007/opf\" version=\"3.0\" unique-identifier=\"BookId\">\n" + "  <metadata xmlns:dc=\"http://purl.org/dc/elements/1.1/\">\n" + "    <dc:title>Test Title</dc:title>\n" + "  </metadata>\n" + "</package>");
                }
            }

            var metadata = EpubMetadataReader.GetMetadata(epubPath);

            metadata.Title.ShouldBe("Test Title");
        }
        finally
        {
            TryDeleteDirectory(tempDir);
        }
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "tomeshelf-tests", Guid.NewGuid()
                                                                           .ToString("N"));
        Directory.CreateDirectory(path);

        return path;
    }

    private static void TryDeleteDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        catch { }
    }
}