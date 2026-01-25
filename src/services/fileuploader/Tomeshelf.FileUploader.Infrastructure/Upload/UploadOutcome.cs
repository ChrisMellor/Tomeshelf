namespace Tomeshelf.FileUploader.Infrastructure.Upload;

public sealed record UploadOutcome(bool Uploaded, string FileId);
