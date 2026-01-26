using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.FileUploader.Application.Abstractions.Messaging;
using Tomeshelf.FileUploader.Application.Abstractions.Upload;
using Tomeshelf.FileUploader.Application.Features.Uploads.Models;

namespace Tomeshelf.FileUploader.Application.Features.Uploads.Commands;

public sealed class UploadBundleArchiveCommandHandler : ICommandHandler<UploadBundleArchiveCommand, BundleUploadResult>
{
    private readonly IHumbleBundleUploadService _uploadService;

    public UploadBundleArchiveCommandHandler(IHumbleBundleUploadService uploadService)
    {
        _uploadService = uploadService;
    }

    public Task<BundleUploadResult> Handle(UploadBundleArchiveCommand command, CancellationToken cancellationToken)
    {
        return _uploadService.UploadAsync(command.ArchiveStream, command.ArchiveFileName, command.OverrideOptions, cancellationToken);
    }
}
