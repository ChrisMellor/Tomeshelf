namespace Tomeshelf.Web.Models.Bundles;

public sealed class BookUploadResultModel
{
    public string BundleName { get; init; } = string.Empty;

    public string BookTitle { get; init; } = string.Empty;

    public int FilesUploaded { get; init; }

    public int FilesSkipped { get; init; }
}