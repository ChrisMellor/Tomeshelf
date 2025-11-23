using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Options;

namespace Tomeshelf.Infrastructure.Bundles.Upload;

public interface IHumbleBundleUploadService
{
    Task<BundleUploadResult> UploadAsync(Stream archiveStream, string archiveFileName, GoogleDriveOptions overrideOptions, CancellationToken cancellationToken);
}