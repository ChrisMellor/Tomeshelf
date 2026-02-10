using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.FileUploader.Application.Abstractions.Upload;
using Tomeshelf.FileUploader.Application.Features.Uploads.Models;

namespace Tomeshelf.FileUploader.Application.Features.Uploads.Commands;

public sealed class UploadBundleArchiveCommandHandler : ICommandHandler<UploadBundleArchiveCommand, BundleUploadResult>
{
    private readonly IHumbleBundleUploadService _uploadService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UploadBundleArchiveCommandHandler" /> class.
    /// </summary>
    /// <param name="uploadService">The upload service.</param>
    public UploadBundleArchiveCommandHandler(IHumbleBundleUploadService uploadService)
    {
        _uploadService = uploadService;
    }

    /// <summary>
    ///     Handles.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public Task<BundleUploadResult> Handle(UploadBundleArchiveCommand command, CancellationToken cancellationToken)
    {
        return _uploadService.UploadAsync(command.ArchiveStream, command.ArchiveFileName, command.OverrideOptions, cancellationToken);
    }
}