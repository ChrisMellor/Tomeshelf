using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.FileUploader.Application.Features.Uploads.Models;

namespace Tomeshelf.FileUploader.Application.Abstractions.Upload;

public interface IHumbleBundleUploadService
{
    Task<BundleUploadResult> UploadAsync(Stream archiveStream, string archiveFileName, GoogleDriveOptions? overrideOptions, CancellationToken cancellationToken);
}