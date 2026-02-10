using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Tomeshelf.FileUploader.Infrastructure.Upload;

public interface IGoogleDriveClient : IDisposable
{
    /// <summary>
    ///     Ensures the folder path asynchronously.
    /// </summary>
    /// <param name="folderPath">The folder path.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    Task<string> EnsureFolderPathAsync(string folderPath, CancellationToken cancellationToken);

    /// <summary>
    ///     Uploads the file asynchronously.
    /// </summary>
    /// <param name="parentFolderId">The parent folder id.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="content">The content.</param>
    /// <param name="contentLength">The content length.</param>
    /// <param name="contentType">The content type.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    Task<UploadOutcome> UploadFileAsync(string parentFolderId, string fileName, Stream content, long contentLength, string? contentType, CancellationToken cancellationToken);
}