using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Tomeshelf.FileUploader.Infrastructure.Upload;

public interface IGoogleDriveClient : IDisposable
{
    Task<string> EnsureFolderPathAsync(string folderPath, CancellationToken cancellationToken);

    Task<UploadOutcome> UploadFileAsync(string parentFolderId, string fileName, Stream content, long contentLength, string? contentType, CancellationToken cancellationToken);
}