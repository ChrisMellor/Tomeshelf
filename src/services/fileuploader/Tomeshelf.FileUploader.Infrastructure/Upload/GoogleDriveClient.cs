using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.FileUploader.Application;
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace Tomeshelf.FileUploader.Infrastructure.Upload;

public interface IGoogleDriveClient : IDisposable
{
    Task<string> EnsureFolderPathAsync(string folderPath, CancellationToken cancellationToken);

    Task<UploadOutcome> UploadFileAsync(string parentFolderId, string fileName, Stream content, long contentLength, string? contentType, CancellationToken cancellationToken);
}

public sealed record UploadOutcome(bool Uploaded, string FileId);

internal sealed class GoogleDriveClient : IGoogleDriveClient, IDisposable
{
    private readonly DriveService _drive;
    private readonly ConcurrentDictionary<string, string> _folderCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<GoogleDriveClient> _logger;
    private readonly GoogleDriveOptions _options;
    private readonly string? _sharedDriveId;
    private bool _disposed;
    private string? _rootFolderId;
    private bool _validatedRoot;

    public GoogleDriveClient(IOptions<GoogleDriveOptions> options, ILogger<GoogleDriveClient> logger)
    {
        _options = options.Value;
        _sharedDriveId = string.IsNullOrWhiteSpace(_options.SharedDriveId)
            ? null
            : _options.SharedDriveId;
        _rootFolderId = string.IsNullOrWhiteSpace(_options.RootFolderId)
            ? null
            : _options.RootFolderId;
        _logger = logger;
        _drive = BuildDriveService(_options);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _drive.Dispose();
        _disposed = true;
    }

    public async Task<string> EnsureFolderPathAsync(string folderPath, CancellationToken cancellationToken)
    {
        var segments = folderPath.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var currentPath = string.Empty;
        var parentId = string.IsNullOrWhiteSpace(_sharedDriveId)
            ? string.IsNullOrWhiteSpace(_rootFolderId)
                ? "root"
                : _rootFolderId
            : _sharedDriveId;

        if (!_validatedRoot && !string.IsNullOrWhiteSpace(_rootFolderId))
        {
            parentId = await ValidateRootFolderAsync(parentId, cancellationToken);
            _validatedRoot = true;
            _rootFolderId = parentId;
        }

        if (segments.Length == 0)
        {
            if (!string.IsNullOrWhiteSpace(_rootFolderId))
            {
                return parentId; // anchor directly to the provided folder id
            }

            throw new InvalidOperationException("Folder path must contain at least one segment.");
        }

        foreach (var segment in segments)
        {
            currentPath = string.IsNullOrEmpty(currentPath)
                ? segment
                : $"{currentPath}/{segment}";

            if (_folderCache.TryGetValue(currentPath, out var cached))
            {
                parentId = cached;

                continue;
            }

            var folderId = await FindFolderAsync(parentId, segment, cancellationToken) ?? await CreateFolderAsync(parentId, segment, cancellationToken);

            _folderCache[currentPath] = folderId;
            parentId = folderId;
        }

        return parentId;
    }

    public async Task<UploadOutcome> UploadFileAsync(string parentFolderId, string fileName, Stream content, long contentLength, string? contentType, CancellationToken cancellationToken)
    {
        _ = content ?? throw new ArgumentNullException(nameof(content));
        if (string.IsNullOrWhiteSpace(parentFolderId))
        {
            throw new ArgumentException("Parent folder ID must be supplied.", nameof(parentFolderId));
        }

        try
        {
            var existing = await FindFileAsync(parentFolderId, fileName, cancellationToken);
            var mimeType = string.IsNullOrWhiteSpace(contentType)
                ? "application/octet-stream"
                : contentType;

            if (existing is not null && existing.Size.HasValue && (existing.Size.Value == contentLength) && content.CanSeek)
            {
                _logger.LogInformation("Skipping upload for {File} in folder {FolderId} because a matching file already exists with the same size.", fileName, parentFolderId);

                return new UploadOutcome(false, existing.Id);
            }

            if (content.CanSeek)
            {
                content.Seek(0, SeekOrigin.Begin);
            }

            if (existing is not null)
            {
                var update = _drive.Files.Update(new DriveFile { Name = fileName }, existing.Id, content, mimeType);
                update.Fields = "id";
                update.SupportsAllDrives = true;
                update.ChunkSize = ResumableUpload.MinimumChunkSize * 32; // ~8MB chunks
                var result = await update.UploadAsync(cancellationToken);
                ValidateUpload(result, fileName, existing.Id);

                _logger.LogInformation("Replaced Google Drive file {FileId} with new content for {FileName}.", existing.Id, fileName);

                return new UploadOutcome(true, existing.Id);
            }
            else
            {
                var create = _drive.Files.Create(new DriveFile
                {
                    Name = fileName,
                    Parents = new[] { parentFolderId }
                }, content, mimeType);

                create.Fields = "id";
                create.SupportsAllDrives = true;
                create.ChunkSize = ResumableUpload.MinimumChunkSize * 32; // ~8MB chunks
                var result = await create.UploadAsync(cancellationToken);
                ValidateUpload(result, fileName, parentFolderId);

                var id = create.ResponseBody?.Id ?? throw new InvalidOperationException("Google Drive did not return a file id.");
                _logger.LogInformation("Uploaded {FileName} to Google Drive folder {FolderId} as {FileId}.", fileName, parentFolderId, id);

                return new UploadOutcome(true, id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google Drive upload failed for {FileName} (length {Length}) to parent {ParentId}.", fileName, contentLength, parentFolderId);

            throw;
        }
    }

    private DriveService BuildDriveService(GoogleDriveOptions options)
    {
        var credential = CreateCredential(options);

        var initializer = new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = string.IsNullOrWhiteSpace(options.ApplicationName)
                ? "Tomeshelf"
                : options.ApplicationName
        };

        var service = new DriveService(initializer);
        service.HttpClient.Timeout = TimeSpan.FromMinutes(30);

        return service;
    }

    private static IConfigurableHttpClientInitializer CreateCredential(GoogleDriveOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.RefreshToken) && !string.IsNullOrWhiteSpace(options.ClientId) && !string.IsNullOrWhiteSpace(options.ClientSecret))
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = options.ClientId,
                    ClientSecret = options.ClientSecret
                },
                Scopes = new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive }
            });

            var token = new TokenResponse { RefreshToken = options.RefreshToken };

            return new UserCredential(flow, options.UserEmail ?? "user", token);
        }

        throw new InvalidOperationException("GoogleDrive OAuth credentials were not configured. Supply ClientId, ClientSecret, and RefreshToken.");
    }

    private async Task<string> CreateFolderAsync(string parentId, string name, CancellationToken cancellationToken)
    {
        var request = _drive.Files.Create(new DriveFile
        {
            Name = name,
            MimeType = "application/vnd.google-apps.folder",
            Parents = new[] { parentId }
        });

        request.Fields = "id";
        request.SupportsAllDrives = true;
        var created = await request.ExecuteAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(created.Id))
        {
            throw new InvalidOperationException($"Failed to create folder '{name}' in parent '{parentId}'.");
        }

        _logger.LogInformation("Created Google Drive folder '{Folder}' under parent {Parent}.", name, parentId);

        return created.Id;
    }

    private static string EscapeQueryLiteral(string value)
    {
        return value.Replace("'", "\\'", StringComparison.Ordinal);
    }

    private async Task<DriveFile?> FindFileAsync(string parentId, string fileName, CancellationToken cancellationToken)
    {
        var list = _drive.Files.List();
        list.Q = $"name = '{EscapeQueryLiteral(fileName)}' and '{parentId}' in parents and trashed = false";
        list.Fields = "files(id, size)";
        list.Spaces = "drive";
        list.SupportsAllDrives = true;
        list.IncludeItemsFromAllDrives = true;
        if (!string.IsNullOrWhiteSpace(_sharedDriveId))
        {
            list.Corpora = "drive";
            list.DriveId = _sharedDriveId;
        }

        var response = await list.ExecuteAsync(cancellationToken);

        return response.Files?.FirstOrDefault();
    }

    private async Task<string?> FindFolderAsync(string parentId, string name, CancellationToken cancellationToken)
    {
        var list = _drive.Files.List();
        list.Q = $"mimeType = 'application/vnd.google-apps.folder' and name = '{EscapeQueryLiteral(name)}' and '{parentId}' in parents and trashed = false";
        list.Fields = "files(id, name)";
        list.Spaces = "drive";
        list.SupportsAllDrives = true;
        list.IncludeItemsFromAllDrives = true;
        if (!string.IsNullOrWhiteSpace(_sharedDriveId))
        {
            list.Corpora = "drive";
            list.DriveId = _sharedDriveId;
        }

        var response = await list.ExecuteAsync(cancellationToken);

        return response.Files?.FirstOrDefault()
                      ?.Id;
    }

    private async Task<string> ValidateRootFolderAsync(string folderId, CancellationToken cancellationToken)
    {
        try
        {
            var request = _drive.Files.Get(folderId);
            request.Fields = "id, name, mimeType, shortcutDetails";
            request.SupportsAllDrives = true;
            var file = await request.ExecuteAsync(cancellationToken);
            if (file is null || (file.MimeType != "application/vnd.google-apps.folder"))
            {
                if (string.Equals(file?.MimeType, "application/vnd.google-apps.shortcut", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(file.ShortcutDetails?.TargetId))
                {
                    var targetId = file.ShortcutDetails.TargetId;
                    _logger.LogInformation("RootFolderId {FolderId} is a shortcut; resolving to target {TargetId}.", folderId, targetId);

                    return await ValidateRootFolderAsync(targetId, cancellationToken);
                }

                throw new InvalidOperationException($"Configured RootFolderId '{folderId}' is not a folder or is inaccessible.");
            }

            return file.Id ?? folderId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to access configured RootFolderId {FolderId}. Ensure the folder exists and the authorised user has access.", folderId);

            throw;
        }
    }

    private void ValidateUpload(IUploadProgress progress, string fileName, string targetId)
    {
        if (progress.Exception is not null)
        {
            _logger.LogError(progress.Exception, "Google Drive upload for {FileName} failed targeting {TargetId} with status {Status}.", fileName, targetId, progress.Status);

            throw progress.Exception;
        }

        if (progress.Status != UploadStatus.Completed)
        {
            var message = $"Google Drive upload for '{fileName}' did not complete. Status: {progress.Status}";
            _logger.LogError(message + " (target {TargetId}).", targetId);

            throw new InvalidOperationException(message);
        }
    }
}