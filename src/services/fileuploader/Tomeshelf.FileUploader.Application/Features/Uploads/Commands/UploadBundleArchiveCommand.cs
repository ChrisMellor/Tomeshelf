using System.IO;
using Tomeshelf.FileUploader.Application.Abstractions.Messaging;
using Tomeshelf.FileUploader.Application.Features.Uploads.Models;

namespace Tomeshelf.FileUploader.Application.Features.Uploads.Commands;

public sealed record UploadBundleArchiveCommand(Stream ArchiveStream, string ArchiveFileName, GoogleDriveOptions? OverrideOptions) : ICommand<BundleUploadResult>;
