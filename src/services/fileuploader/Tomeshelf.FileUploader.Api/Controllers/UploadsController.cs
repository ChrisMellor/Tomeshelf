using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.FileUploader.Application;
using Tomeshelf.FileUploader.Application.Features.Uploads.Commands;
using Tomeshelf.FileUploader.Application.Features.Uploads.Models;

namespace Tomeshelf.FileUploader.Api.Controllers;

[ApiController]
[Route("uploads")]
public sealed class UploadsController : ControllerBase
{
    private readonly GoogleDriveOptions _defaultDriveOptions;
    private readonly ILogger<UploadsController> _logger;
    private readonly ICommandHandler<UploadBundleArchiveCommand, BundleUploadResult> _uploadHandler;

    public UploadsController(ICommandHandler<UploadBundleArchiveCommand, BundleUploadResult> uploadHandler, IOptions<GoogleDriveOptions> driveOptions, ILogger<UploadsController> logger)
    {
        _uploadHandler = uploadHandler;
        _defaultDriveOptions = driveOptions.Value;
        _logger = logger;
    }

    /// <summary>
    ///     Receives a Humble Bundle archive (zip) and uploads its contents to Google Drive under the configured root path.
    /// </summary>
    /// <param name="archive">A zip archive containing the bundle files exactly as downloaded from Humble Bundle.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(1_073_741_824)]
    [ProducesResponseType(typeof(BundleUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BundleUploadResponse>> Upload([FromForm] IFormFile archive, [FromForm] OAuthCredentials? credentials, CancellationToken cancellationToken = default)
    {
        if (archive is null || (archive.Length == 0))
        {
            return BadRequest("A bundle archive (.zip) file is required.");
        }

        _logger.LogInformation("Received bundle upload '{FileName}' ({Length} bytes).", archive.FileName, archive.Length);

        await using var stream = archive.OpenReadStream();
        var overrideOptions = ToOptions(credentials);
        if (overrideOptions is null)
        {
            return BadRequest("Google Drive OAuth credentials are missing. Authorise via the web app and try again.");
        }

        var command = new UploadBundleArchiveCommand(stream, archive.FileName, overrideOptions);
        var result = await _uploadHandler.Handle(command, cancellationToken);

        return Ok(BundleUploadResponse.FromResult(result));
    }

    private GoogleDriveOptions? ToOptions(OAuthCredentials? creds)
    {
        if (creds is null || string.IsNullOrWhiteSpace(creds.ClientId) || string.IsNullOrWhiteSpace(creds.ClientSecret) || string.IsNullOrWhiteSpace(creds.RefreshToken))
        {
            return string.IsNullOrWhiteSpace(_defaultDriveOptions.ClientId) || string.IsNullOrWhiteSpace(_defaultDriveOptions.ClientSecret) || string.IsNullOrWhiteSpace(_defaultDriveOptions.RefreshToken)
                ? null
                : _defaultDriveOptions;
        }

        return new GoogleDriveOptions
        {
            ApplicationName = _defaultDriveOptions.ApplicationName,
            RootFolderPath = _defaultDriveOptions.RootFolderPath,
            RootFolderId = _defaultDriveOptions.RootFolderId,
            ClientId = creds.ClientId,
            ClientSecret = creds.ClientSecret,
            RefreshToken = creds.RefreshToken,
            UserEmail = string.IsNullOrWhiteSpace(creds.UserEmail)
                ? _defaultDriveOptions.UserEmail
                : creds.UserEmail,
            SharedDriveId = _defaultDriveOptions.SharedDriveId
        };
    }

    public sealed record BundleUploadResponse(DateTimeOffset UploadedAtUtc, int BundlesProcessed, int BooksProcessed, int FilesUploaded, int FilesSkipped, IReadOnlyList<BookUploadResponse> Books)
    {
        public static BundleUploadResponse FromResult(BundleUploadResult result)
        {
            var books = result.Books
                              .Select(BookUploadResponse.FromResult)
                              .ToList();

            return new BundleUploadResponse(result.UploadedAtUtc, result.BundlesProcessed, result.BooksProcessed, result.FilesUploaded, result.FilesSkipped, books);
        }
    }

    public sealed record BookUploadResponse(string BundleName, string BookTitle, int FilesUploaded, int FilesSkipped)
    {
        public static BookUploadResponse FromResult(BookUploadResult result)
        {
            return new BookUploadResponse(result.BundleName, result.BookTitle, result.FilesUploaded, result.FilesSkipped);
        }
    }

    public sealed class OAuthCredentials
    {
        public string? ClientId { get; init; }

        public string? ClientSecret { get; init; }

        public string? RefreshToken { get; init; }

        public string? UserEmail { get; init; }
    }
}