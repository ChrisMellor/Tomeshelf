using Tomeshelf.FileUploader.Application;

namespace Tomeshelf.FileUploader.Infrastructure.Upload;

public interface IGoogleDriveClientFactory
{
    /// <summary>
    ///     Creates.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <returns>The result of the operation.</returns>
    IGoogleDriveClient Create(GoogleDriveOptions options);
}