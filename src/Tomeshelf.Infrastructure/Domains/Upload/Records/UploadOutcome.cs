namespace Tomeshelf.Infrastructure.Domains.Upload.Records;

public sealed record UploadOutcome
{
    public UploadOutcome(bool uploaded, string fileId)
    {
        Uploaded = uploaded;
        FileId = fileId;
    }

    public bool Uploaded { get; init; }

    public string FileId { get; init; }

    public void Deconstruct(out bool uploaded, out string fileId)
    {
        uploaded = Uploaded;
        fileId = FileId;
    }
}