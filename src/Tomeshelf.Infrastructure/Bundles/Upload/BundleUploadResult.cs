using System;
using System.Collections.Generic;
using System.Linq;

namespace Tomeshelf.Infrastructure.Bundles.Upload;

public sealed record BundleUploadResult
{
    public BundleUploadResult(DateTimeOffset uploadedAtUtc, int bundlesProcessed, int booksProcessed, int filesUploaded, int filesSkipped, IReadOnlyList<BookUploadResult> books)
    {
        UploadedAtUtc = uploadedAtUtc;
        BundlesProcessed = bundlesProcessed;
        BooksProcessed = booksProcessed;
        FilesUploaded = filesUploaded;
        FilesSkipped = filesSkipped;
        Books = books;
    }

    public DateTimeOffset UploadedAtUtc { get; init; }

    public int BundlesProcessed { get; init; }

    public int BooksProcessed { get; init; }

    public int FilesUploaded { get; init; }

    public int FilesSkipped { get; init; }

    public IReadOnlyList<BookUploadResult> Books { get; init; }

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

    public void Deconstruct(out DateTimeOffset uploadedAtUtc, out int bundlesProcessed, out int booksProcessed, out int filesUploaded, out int filesSkipped,
            out IReadOnlyList<BookUploadResult> books)
    {
        uploadedAtUtc = UploadedAtUtc;
        bundlesProcessed = BundlesProcessed;
        booksProcessed = BooksProcessed;
        filesUploaded = FilesUploaded;
        filesSkipped = FilesSkipped;
        books = Books;
    }
}