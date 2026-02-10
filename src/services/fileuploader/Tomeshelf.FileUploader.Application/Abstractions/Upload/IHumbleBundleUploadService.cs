using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.FileUploader.Application.Features.Uploads.Models;

namespace Tomeshelf.FileUploader.Application.Abstractions.Upload;

public interface IHumbleBundleUploadService
{
    /// <summary>
    ///     Uploads asynchronously.
    /// </summary>
    /// <param name="archiveStream">The archive stream.</param>
    /// <param name="archiveFileName">The archive file name.</param>
    /// <param name="overrideOptions">The override options.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    Task<BundleUploadResult> UploadAsync(Stream archiveStream, string archiveFileName, GoogleDriveOptions? overrideOptions, CancellationToken cancellationToken);
}