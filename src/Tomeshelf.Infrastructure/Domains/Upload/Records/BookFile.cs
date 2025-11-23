namespace Tomeshelf.Infrastructure.Domains.Upload.Records;

public sealed record BookFile
{
    public BookFile(string fullPath, string targetFileName, long length)
    {
        FullPath = fullPath;
        TargetFileName = targetFileName;
        Length = length;
    }

    public string FullPath { get; init; }

    public string TargetFileName { get; init; }

    public long Length { get; init; }

    public void Deconstruct(out string fullPath, out string targetFileName, out long length)
    {
        fullPath = FullPath;
        targetFileName = TargetFileName;
        length = Length;
    }
}