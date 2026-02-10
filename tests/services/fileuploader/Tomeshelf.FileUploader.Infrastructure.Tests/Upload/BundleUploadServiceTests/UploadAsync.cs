using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;
using System.IO.Compression;
using Tomeshelf.FileUploader.Application;
using Tomeshelf.FileUploader.Infrastructure.Upload;

namespace Tomeshelf.FileUploader.Infrastructure.Tests.Upload.BundleUploadServiceTests;

public class UploadAsync
{
    /// <summary>
    ///     Throws the when no files found.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ThrowsWhenNoFilesFound()
    {
        // Arrange
        using var archive = CreateZipStream();
        var organiser = new BundleFileOrganiser();
        var factory = new RecordingDriveFactory();
        var options = Options.Create(new GoogleDriveOptions
        {
            RootFolderPath = "Root",
            ClientId = "client",
            ClientSecret = "secret",
            RefreshToken = "refresh"
        });
        var service = new BundleUploadService(organiser, factory, options, NullLogger<BundleUploadService>.Instance);

        // Act
        var action = async () => await service.UploadAsync(archive, "bundle.zip", null, CancellationToken.None);

        // Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(action);
        exception.Message.ShouldContain("No files were found in the uploaded bundle.");
        factory.CreateCalls.ShouldBe(0);
    }

    /// <summary>
    ///     Uploads the files and uses override options.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UploadsFilesAndUsesOverrideOptions()
    {
        // Arrange
        using var archive = CreateZipStream(("My Bundle by Author/BookOne.txt", "content"), ("My Bundle by Author/BookOne_supplement.zip", "supplement"));

        var organiser = new BundleFileOrganiser();
        var factory = new RecordingDriveFactory();
        factory.Client.UploadOutcomeFactory = (folder, fileName) => fileName.Contains("Supplement", StringComparison.OrdinalIgnoreCase)
            ? new UploadOutcome(false, "skipped")
            : new UploadOutcome(true, "uploaded");

        var options = Options.Create(new GoogleDriveOptions
        {
            RootFolderPath = "DefaultRoot",
            ClientId = "default-client",
            ClientSecret = "default-secret",
            RefreshToken = "default-refresh",
            UserEmail = "default@example.com"
        });

        var overrideOptions = new GoogleDriveOptions
        {
            RootFolderPath = "OverrideRoot",
            ClientId = "override-client",
            ClientSecret = "override-secret",
            RefreshToken = "override-refresh"
        };

        var service = new BundleUploadService(organiser, factory, options, NullLogger<BundleUploadService>.Instance);

        // Act
        var result = await service.UploadAsync(archive, "bundle.zip", overrideOptions, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.BundlesProcessed.ShouldBe(1);
        result.BooksProcessed.ShouldBe(1);
        result.FilesUploaded.ShouldBe(1);
        result.FilesSkipped.ShouldBe(1);
        result.Books.ShouldHaveSingleItem();

        factory.Options.ShouldNotBeNull();
        factory.Options!.RootFolderPath.ShouldBe("OverrideRoot");
        factory.Options.ClientId.ShouldBe("override-client");
        factory.Options.ClientSecret.ShouldBe("override-secret");
        factory.Options.RefreshToken.ShouldBe("override-refresh");
        factory.Options.UserEmail.ShouldBe("default@example.com");
        factory.Client.FolderPaths.ShouldHaveSingleItem();
        factory.Client
               .FolderPaths[0]
               .ShouldBe("OverrideRoot/My Bundle by Author/Unknown Title");
    }

    /// <summary>
    ///     Creates the zip stream.
    /// </summary>
    /// <param name="entries">The entries.</param>
    /// <returns>The result of the operation.</returns>
    private static MemoryStream CreateZipStream(params (string Path, string Content)[] entries)
    {
        var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            foreach (var entry in entries)
            {
                var zipEntry = zip.CreateEntry(entry.Path);
                using var writer = new StreamWriter(zipEntry.Open());
                writer.Write(entry.Content);
            }
        }

        stream.Position = 0;

        return stream;
    }

    private sealed class RecordingDriveFactory : IGoogleDriveClientFactory
    {
        public GoogleDriveOptions? Options { get; private set; }

        public StubDriveClient Client { get; } = new();

        public int CreateCalls { get; private set; }

        /// <summary>
        ///     Creates.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>The result of the operation.</returns>
        public IGoogleDriveClient Create(GoogleDriveOptions options)
        {
            Options = options;
            CreateCalls++;

            return Client;
        }
    }

    private sealed class StubDriveClient : IGoogleDriveClient
    {
        public List<string> FolderPaths { get; } = new();

        public List<string> UploadedFiles { get; } = new();

        public Func<string, string, UploadOutcome>? UploadOutcomeFactory { get; set; }

        /// <summary>
        ///     Releases resources used by this instance.
        /// </summary>
        public void Dispose() { }

        /// <summary>
        ///     Ensures the folder path asynchronously.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
        public Task<string> EnsureFolderPathAsync(string folderPath, CancellationToken cancellationToken)
        {
            FolderPaths.Add(folderPath);

            return Task.FromResult("folder-id");
        }

        /// <summary>
        ///     Uploads the file asynchronously.
        /// </summary>
        /// <param name="parentFolderId">The parent folder id.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="content">The content.</param>
        /// <param name="contentLength">The content length.</param>
        /// <param name="contentType">The content type.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
        public Task<UploadOutcome> UploadFileAsync(string parentFolderId, string fileName, Stream content, long contentLength, string? contentType, CancellationToken cancellationToken)
        {
            UploadedFiles.Add(fileName);
            var outcome = UploadOutcomeFactory?.Invoke(parentFolderId, fileName) ?? new UploadOutcome(true, "file-id");

            return Task.FromResult(outcome);
        }
    }
}