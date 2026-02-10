using System.Text;
using Shouldly;
using Tomeshelf.FileUploader.Infrastructure.Upload;

namespace Tomeshelf.FileUploader.Infrastructure.Tests.Upload.MobiMetadataReaderTests;

public class GetMetadata
{
    /// <summary>
    ///     Falls the back to pdb title when no exth.
    /// </summary>
    [Fact]
    public void FallsBackToPdbTitleWhenNoExth()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var path = Path.Combine(tempDir, "book.mobi");

        try
        {
            var data = new byte[64];
            var pdbTitle = Encoding.Latin1.GetBytes("Fallback Title");
            Array.Copy(pdbTitle, data, pdbTitle.Length);
            data[pdbTitle.Length] = 0;

            File.WriteAllBytes(path, data);

            // Act
            var metadata = MobiMetadataReader.GetMetadata(path);

            // Assert
            metadata.Title.ShouldBe("Fallback Title");
        }
        finally
        {
            TryDeleteDirectory(tempDir);
        }
    }

    /// <summary>
    ///     Uses the exth title when present.
    /// </summary>
    [Fact]
    public void UsesExthTitleWhenPresent()
    {
        // Arrange
        var tempDir = CreateTempDirectory();
        var path = Path.Combine(tempDir, "book.mobi");

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

            // Act
            var metadata = MobiMetadataReader.GetMetadata(path);

            // Assert
            metadata.Title.ShouldBe("Mobi Title");
        }
        finally
        {
            TryDeleteDirectory(tempDir);
        }
    }

    /// <summary>
    ///     Creates the temp directory.
    /// </summary>
    /// <returns>The resulting string.</returns>
    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "tomeshelf-tests", Guid.NewGuid()
                                                                           .ToString("N"));
        Directory.CreateDirectory(path);

        return path;
    }

    /// <summary>
    ///     Attempts to delete a directory.
    /// </summary>
    /// <param name="path">The path.</param>
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

    /// <summary>
    ///     WriteBs the e 32.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="value">The value.</param>
    private static void WriteBE32(byte[] data, int offset, uint value)
    {
        data[offset] = (byte)((value >> 24) & 0xFF);
        data[offset + 1] = (byte)((value >> 16) & 0xFF);
        data[offset + 2] = (byte)((value >> 8) & 0xFF);
        data[offset + 3] = (byte)(value & 0xFF);
    }
}
