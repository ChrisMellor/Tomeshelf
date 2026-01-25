using Tomeshelf.FileUploader.Application;

namespace Tomeshelf.FileUploader.Infrastructure.Upload;

public interface IGoogleDriveClientFactory
{
    IGoogleDriveClient Create(GoogleDriveOptions options);
}
