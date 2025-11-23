using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tomeshelf.Application.Options;
using Tomeshelf.FileUploader.Api.Records;
using Tomeshelf.Infrastructure.Bundles.Upload;

namespace Tomeshelf.FileUploader.Api.Controllers;

[ApiController]
[Route("uploads")]
public sealed class UploadsController : ControllerBase
{
    private readonly GoogleDriveOptions _defaultDriveOptions;
    private readonly ILogger<UploadsController> _logger;
    private readonly IHumbleBundleUploadService _uploadService;

    public UploadsController(IHumbleBundleUploadService uploadService, IOptions<GoogleDriveOptions> driveOptions, ILogger<UploadsController> logger)
    {
        _uploadService = uploadService;
        _defaultDriveOptions = driveOptions.Value;
        _logger = logger;
    }

    /// <summary>
    ///     Receives a Humble Bundle archive (zip) and uploads its contents to Google Drive under the configured root path.
    /// </summary>
    /// <param name="archive">A zip archive containing the bundle files exactly as downloaded from Humble Bundle.</param>
    /// <param name="credentials"></param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(1_073_741_824)] // ~1GB
    [ProducesResponseType(typeof(BundleUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BundleUploadResponse>> Upload([FromForm] IFormFile archive, [FromForm] OAuthCredentials credentials,
            CancellationToken cancellationToken = default)
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

        var result = await _uploadService.UploadAsync(stream, archive.FileName, overrideOptions, cancellationToken);

        return Ok(BundleUploadResponse.FromResult(result));
    }

    private GoogleDriveOptions ToOptions(OAuthCredentials credentials)
    {
        if (credentials is null ||
            string.IsNullOrWhiteSpace(credentials.ClientId) ||
            string.IsNullOrWhiteSpace(credentials.ClientSecret) ||
            string.IsNullOrWhiteSpace(credentials.RefreshToken))
        {
            return string.IsNullOrWhiteSpace(_defaultDriveOptions.ClientId) ||
                   string.IsNullOrWhiteSpace(_defaultDriveOptions.ClientSecret) ||
                   string.IsNullOrWhiteSpace(_defaultDriveOptions.RefreshToken)
                    ? null
                    : _defaultDriveOptions;
        }

        return new GoogleDriveOptions
        {
                ApplicationName = _defaultDriveOptions.ApplicationName,
                RootFolderPath = _defaultDriveOptions.RootFolderPath,
                RootFolderId = _defaultDriveOptions.RootFolderId,
                ClientId = credentials.ClientId,
                ClientSecret = credentials.ClientSecret,
                RefreshToken = credentials.RefreshToken,
                UserEmail = string.IsNullOrWhiteSpace(credentials.UserEmail)
                        ? _defaultDriveOptions.UserEmail
                        : credentials.UserEmail,
                SharedDriveId = _defaultDriveOptions.SharedDriveId
        };
    }
}