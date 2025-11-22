using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Models.Bundles;

namespace Tomeshelf.Web.Services;

/// <summary>
///     Abstraction for uploading bundle archives to the file uploader API.
/// </summary>
public interface IFileUploadsApi
{
    /// <summary>
    ///     Uploads a Humble Bundle archive to the backend for processing and Google Drive upload.
    /// </summary>
    /// <param name="archiveStream">Stream containing the bundle archive (typically a zip).</param>
    /// <param name="fileName">Original file name for the archive.</param>
    /// <param name="cancellationToken">Cancellation token for the HTTP request.</param>
    Task<BundleUploadResultModel> UploadBundleAsync(Stream archiveStream, string fileName, GoogleDriveAuthModel? auth, CancellationToken cancellationToken);
}
