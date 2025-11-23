using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Options;
using Tomeshelf.Infrastructure.Domains.Upload.Records;

namespace Tomeshelf.Infrastructure.Domains.Upload.Services;

public interface IBundleUploadService
{
    Task<BundleUploadResult> UploadAsync(Stream archiveStream, string archiveFileName, GoogleDriveOptions overrideOptions, CancellationToken cancellationToken);
}