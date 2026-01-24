using System;
using System.Collections.Generic;
using System.Linq;

namespace Tomeshelf.FileUploader.Infrastructure.Upload;

public sealed record BundleUploadResult(DateTimeOffset UploadedAtUtc, int BundlesProcessed, int BooksProcessed, int FilesUploaded, int FilesSkipped, IReadOnlyList<BookUploadResult> Books)
{
    public static BundleUploadResult FromBooks(IEnumerable<BookUploadResult> books, DateTimeOffset uploadedAtUtc)
    {
        var resultList = books.ToList();
        var bundles = resultList.Select(b => b.BundleName)
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .Count();

        var filesUploaded = resultList.Sum(b => b.FilesUploaded);
        var filesSkipped = resultList.Sum(b => b.FilesSkipped);

        return new BundleUploadResult(uploadedAtUtc, bundles, resultList.Count, filesUploaded, filesSkipped, resultList);
    }
}

public sealed record BookUploadResult(string BundleName, string BookTitle, int FilesUploaded, int FilesSkipped);