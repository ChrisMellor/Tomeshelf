using System;
using System.Collections.Generic;

namespace Tomeshelf.Web.Models.Bundles;

public sealed class BundleUploadResultModel
{
    public DateTimeOffset UploadedAtUtc { get; init; }

    public int BundlesProcessed { get; init; }

    public int BooksProcessed { get; init; }

    public int FilesUploaded { get; init; }

    public int FilesSkipped { get; init; }

    public IReadOnlyList<BookUploadResultModel> Books { get; init; } = Array.Empty<BookUploadResultModel>();
}

public sealed class BookUploadResultModel
{
    public string BundleName { get; init; } = string.Empty;

    public string BookTitle { get; init; } = string.Empty;

    public int FilesUploaded { get; init; }

    public int FilesSkipped { get; init; }
}
