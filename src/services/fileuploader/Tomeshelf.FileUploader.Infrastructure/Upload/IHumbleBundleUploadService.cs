using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.FileUploader.Application;

namespace Tomeshelf.FileUploader.Infrastructure.Upload;

public interface IHumbleBundleUploadService
{
    Task<BundleUploadResult> UploadAsync(Stream archiveStream, string archiveFileName, GoogleDriveOptions? overrideOptions, CancellationToken cancellationToken);
}
