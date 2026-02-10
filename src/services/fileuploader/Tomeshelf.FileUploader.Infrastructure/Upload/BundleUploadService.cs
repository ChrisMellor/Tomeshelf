using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tomeshelf.FileUploader.Application;
using Tomeshelf.FileUploader.Application.Abstractions.Upload;
using Tomeshelf.FileUploader.Application.Features.Uploads.Models;

namespace Tomeshelf.FileUploader.Infrastructure.Upload;

public sealed class BundleUploadService : IHumbleBundleUploadService
{
    private readonly IGoogleDriveClientFactory _driveFactory;
    private readonly ILogger<BundleUploadService> _logger;
    private readonly GoogleDriveOptions _options;
    private readonly BundleFileOrganiser _organiser;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BundleUploadService" /> class.
    /// </summary>
    /// <param name="organiser">The organiser.</param>
    /// <param name="driveFactory">The drive factory.</param>
    /// <param name="options">The options.</param>
    /// <param name="logger">The logger.</param>
    public BundleUploadService(BundleFileOrganiser organiser, IGoogleDriveClientFactory driveFactory, IOptions<GoogleDriveOptions> options, ILogger<BundleUploadService> logger)
    {
        _organiser = organiser;
        _driveFactory = driveFactory;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    ///     Uploads asynchronously.
    /// </summary>
    /// <param name="archiveStream">The archive stream.</param>
    /// <param name="archiveFileName">The archive file name.</param>
    /// <param name="overrideOptions">The override options.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public async Task<BundleUploadResult> UploadAsync(Stream archiveStream, string archiveFileName, GoogleDriveOptions? overrideOptions, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archiveStream);

        var workingDirectory = Path.Combine(Path.GetTempPath(), "tomeshelf-bundle-upload", Path.GetRandomFileName());
        Directory.CreateDirectory(workingDirectory);

        try
        {
            var savedArchive = await SaveArchiveAsync(archiveStream, archiveFileName, workingDirectory, cancellationToken);
            var extractionDirectory = Path.Combine(workingDirectory, "extracted");
            Directory.CreateDirectory(extractionDirectory);

            await ExtractAsync(savedArchive, extractionDirectory, cancellationToken);

            var plans = _organiser.BuildPlan(extractionDirectory);
            if (plans.Count == 0)
            {
                throw new InvalidOperationException("No files were found in the uploaded bundle.");
            }

            _logger.LogInformation("Prepared {BookCount} books from uploaded bundle archive '{Archive}'.", plans.Count, archiveFileName);

            var books = new List<BookUploadResult>(plans.Count);
            var resolvedOptions = MergeOptions(overrideOptions);
            var basePath = resolvedOptions.RootFolderPath;

            using var driveClient = _driveFactory.Create(resolvedOptions);

            foreach (var plan in plans)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var folderPath = $"{basePath}/{plan.BundleName}/{plan.BookTitle}";
                var folderId = await driveClient.EnsureFolderPathAsync(folderPath, cancellationToken);

                var uploaded = 0;
                var skipped = 0;

                foreach (var file in plan.Files)
                {
                    await using var stream = File.OpenRead(file.FullPath);
                    var outcome = await driveClient.UploadFileAsync(folderId, file.TargetFileName, stream, file.Length, MimeTypes.GetMimeType(file.TargetFileName), cancellationToken);
                    if (outcome.Uploaded)
                    {
                        uploaded++;
                    }
                    else
                    {
                        skipped++;
                    }
                }

                books.Add(new BookUploadResult(plan.BundleName, plan.BookTitle, uploaded, skipped));
            }

            var result = BundleUploadResult.FromBooks(books, DateTimeOffset.UtcNow);
            _logger.LogInformation("Uploaded bundle archive '{ArchiveName}' to Google Drive. Bundles: {BundleCount}, Books: {BookCount}, Files Uploaded: {Uploaded}, Files Skipped: {Skipped}.", archiveFileName, result.BundlesProcessed, result.BooksProcessed, result.FilesUploaded, result.FilesSkipped);

            return result;
        }
        finally
        {
            TryDeleteDirectory(workingDirectory);
        }
    }

    /// <summary>
    ///     Extracts asynchronously.
    /// </summary>
    /// <param name="archivePath">The archive path.</param>
    /// <param name="destinationDirectory">The destination directory.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private static Task ExtractAsync(string archivePath, string destinationDirectory, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(archivePath);
        if (extension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
        {
            ZipFile.ExtractToDirectory(archivePath, destinationDirectory, true);

            return Task.CompletedTask;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var destination = Path.Combine(destinationDirectory, Path.GetFileName(archivePath));
        File.Copy(archivePath, destination, true);

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Merges the options.
    /// </summary>
    /// <param name="overrideOptions">The override options.</param>
    /// <returns>The result of the operation.</returns>
    private GoogleDriveOptions MergeOptions(GoogleDriveOptions? overrideOptions)
    {
        if (overrideOptions is null)
        {
            return _options;
        }

        return new GoogleDriveOptions
        {
            ApplicationName = _options.ApplicationName,
            RootFolderPath = string.IsNullOrWhiteSpace(overrideOptions.RootFolderPath)
                ? _options.RootFolderPath
                : overrideOptions.RootFolderPath,
            RootFolderId = string.IsNullOrWhiteSpace(overrideOptions.RootFolderId)
                ? _options.RootFolderId
                : overrideOptions.RootFolderId,
            ClientId = overrideOptions.ClientId ?? _options.ClientId,
            ClientSecret = overrideOptions.ClientSecret ?? _options.ClientSecret,
            RefreshToken = overrideOptions.RefreshToken ?? _options.RefreshToken,
            UserEmail = overrideOptions.UserEmail ?? _options.UserEmail,
            SharedDriveId = overrideOptions.SharedDriveId ?? _options.SharedDriveId
        };
    }

    /// <summary>
    ///     Saves the archive asynchronously.
    /// </summary>
    /// <param name="archive">The archive.</param>
    /// <param name="archiveFileName">The archive file name.</param>
    /// <param name="workingDirectory">The working directory.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    private static async Task<string> SaveArchiveAsync(Stream archive, string archiveFileName, string workingDirectory, CancellationToken cancellationToken)
    {
        var safeName = string.IsNullOrWhiteSpace(archiveFileName)
            ? "bundle.zip"
            : Path.GetFileName(archiveFileName);

        var destination = Path.Combine(workingDirectory, safeName);

        await using var file = File.Create(destination);
        await archive.CopyToAsync(file, cancellationToken);

        return destination;
    }

    /// <summary>
    ///     Attempts to delete a directory.
    /// </summary>
    /// <param name="path">The path.</param>
    private void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to clean up temporary bundle upload directory {Path}.", path);
        }
    }
}