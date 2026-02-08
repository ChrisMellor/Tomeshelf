namespace Tomeshelf.FileUploader.Application.Features.Uploads.Models;

public sealed record BookUploadResult(string BundleName, string BookTitle, int FilesUploaded, int FilesSkipped);