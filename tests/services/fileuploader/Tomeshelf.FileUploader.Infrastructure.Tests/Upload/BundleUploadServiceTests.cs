using System.IO.Compression;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Tomeshelf.FileUploader.Application;
using Tomeshelf.FileUploader.Infrastructure.Upload;

namespace Tomeshelf.FileUploader.Infrastructure.Tests.Upload;

public class BundleUploadServiceTests
{
    [Fact]
    public async Task UploadAsync_ThrowsWhenNoFilesFound()
    {
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

        var act = async () => await service.UploadAsync(archive, "bundle.zip", null, CancellationToken.None);

        await act.Should()
                 .ThrowAsync<InvalidOperationException>()
                 .WithMessage("No files were found in the uploaded bundle.*");
        factory.CreateCalls
               .Should()
               .Be(0);
    }

    [Fact]
    public async Task UploadAsync_UploadsFilesAndUsesOverrideOptions()
    {
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

        var result = await service.UploadAsync(archive, "bundle.zip", overrideOptions, CancellationToken.None);

        result.Should()
              .NotBeNull();
        result.BundlesProcessed
              .Should()
              .Be(1);
        result.BooksProcessed
              .Should()
              .Be(1);
        result.FilesUploaded
              .Should()
              .Be(1);
        result.FilesSkipped
              .Should()
              .Be(1);
        result.Books
              .Should()
              .HaveCount(1);

        factory.Options
               .Should()
               .NotBeNull();
        factory.Options!.RootFolderPath
               .Should()
               .Be("OverrideRoot");
        factory.Options
               .ClientId
               .Should()
               .Be("override-client");
        factory.Options
               .ClientSecret
               .Should()
               .Be("override-secret");
        factory.Options
               .RefreshToken
               .Should()
               .Be("override-refresh");
        factory.Options
               .UserEmail
               .Should()
               .Be("default@example.com");
        factory.Client
               .FolderPaths
               .Should()
               .ContainSingle()
               .Which
               .Should()
               .Be("OverrideRoot/My Bundle by Author/Unknown Title");
    }

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

        public void Dispose() { }

        public Task<string> EnsureFolderPathAsync(string folderPath, CancellationToken cancellationToken)
        {
            FolderPaths.Add(folderPath);

            return Task.FromResult("folder-id");
        }

        public Task<UploadOutcome> UploadFileAsync(string parentFolderId, string fileName, Stream content, long contentLength, string? contentType, CancellationToken cancellationToken)
        {
            UploadedFiles.Add(fileName);
            var outcome = UploadOutcomeFactory?.Invoke(parentFolderId, fileName) ?? new UploadOutcome(true, "file-id");

            return Task.FromResult(outcome);
        }
    }
}