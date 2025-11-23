using System;
using System.Collections.Generic;
using System.Linq;
using Tomeshelf.Infrastructure.Bundles.Upload;

namespace Tomeshelf.FileUploader.Api.Records;

public sealed record BundleUploadResponse
{
    public BundleUploadResponse(DateTimeOffset uploadedAtUtc, int bundlesProcessed, int booksProcessed, int filesUploaded, int filesSkipped,
            IReadOnlyList<BookUploadResponse> books)
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

    public IReadOnlyList<BookUploadResponse> Books { get; init; }

    public static BundleUploadResponse FromResult(BundleUploadResult result)
    {
        var books = result.Books.Select(BookUploadResponse.FromResult)
                          .ToList();

        return new BundleUploadResponse(result.UploadedAtUtc, result.BundlesProcessed, result.BooksProcessed, result.FilesUploaded, result.FilesSkipped, books);
    }

    public void Deconstruct(out DateTimeOffset uploadedAtUtc, out int bundlesProcessed, out int booksProcessed, out int filesUploaded, out int filesSkipped,
            out IReadOnlyList<BookUploadResponse> books)
    {
        uploadedAtUtc = UploadedAtUtc;
        bundlesProcessed = BundlesProcessed;
        booksProcessed = BooksProcessed;
        filesUploaded = FilesUploaded;
        filesSkipped = FilesSkipped;
        books = Books;
    }
}