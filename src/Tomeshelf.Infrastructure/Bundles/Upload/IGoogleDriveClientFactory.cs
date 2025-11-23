using Tomeshelf.Application.Options;

namespace Tomeshelf.Infrastructure.Bundles.Upload;

public interface IGoogleDriveClientFactory
{
    IGoogleDriveClient Create(GoogleDriveOptions options);
}