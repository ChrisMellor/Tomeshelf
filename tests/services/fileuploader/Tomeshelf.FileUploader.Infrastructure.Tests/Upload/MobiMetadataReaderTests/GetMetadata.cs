using System.Text;
using Shouldly;
using Tomeshelf.FileUploader.Infrastructure.Upload;

namespace Tomeshelf.FileUploader.Infrastructure.Tests.Upload.MobiMetadataReaderTests;

public class GetMetadata
{
    [Fact]
    public void FallsBackToPdbTitleWhenNoExth()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        // Act
        var path = Path.Combine(tempDir, "book.mobi");

        // Assert
        try
        {
            var data = new byte[64];
            var pdbTitle = Encoding.Latin1.GetBytes("Fallback Title");
            Array.Copy(pdbTitle, data, pdbTitle.Length);
            data[pdbTitle.Length] = 0;

            File.WriteAllBytes(path, data);

            var metadata = MobiMetadataReader.GetMetadata(path);

            metadata.Title.ShouldBe("Fallback Title");
        }
        finally
        {
            TryDeleteDirectory(tempDir);
        }
    }

    [Fact]
    public void UsesExthTitleWhenPresent()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        // Act
        var path = Path.Combine(tempDir, "book.mobi");

        // Assert
        try
        {
            var data = new byte[128];
            var pdbTitle = Encoding.Latin1.GetBytes("Pdb Title");
            Array.Copy(pdbTitle, data, pdbTitle.Length);
            data[pdbTitle.Length] = 0;

            var exthPos = 64;
            var exth = Encoding.ASCII.GetBytes("EXTH");
            Array.Copy(exth, 0, data, exthPos, exth.Length);
            WriteBE32(data, exthPos + 8, 1u);

            var recordStart = exthPos + 12;
            WriteBE32(data, recordStart, 503u);
            var titleBytes = Encoding.UTF8.GetBytes("Mobi Title");
            WriteBE32(data, recordStart + 4, (uint)(8 + titleBytes.Length));
            Array.Copy(titleBytes, 0, data, recordStart + 8, titleBytes.Length);

            File.WriteAllBytes(path, data);

            var metadata = MobiMetadataReader.GetMetadata(path);

            metadata.Title.ShouldBe("Mobi Title");
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

    private static void WriteBE32(byte[] data, int offset, uint value)
    {
        data[offset] = (byte)((value >> 24) & 0xFF);
        data[offset + 1] = (byte)((value >> 16) & 0xFF);
        data[offset + 2] = (byte)((value >> 8) & 0xFF);
        data[offset + 3] = (byte)(value & 0xFF);
    }
}