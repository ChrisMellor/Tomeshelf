namespace Tomeshelf.FileUploader.Infrastructure.Upload;

public sealed record BookFile(string FullPath, string TargetFileName, long Length);