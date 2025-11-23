using System.Collections.Generic;

namespace Tomeshelf.Infrastructure.Bundles.Upload;

public sealed record BookPlan
{
    public BookPlan(string bundleName, string bookTitle, IReadOnlyList<BookFile> files)
    {
        BundleName = bundleName;
        BookTitle = bookTitle;
        Files = files;
    }

    public string BundleName { get; init; }

    public string BookTitle { get; init; }

    public IReadOnlyList<BookFile> Files { get; init; }

    public void Deconstruct(out string bundleName, out string bookTitle, out IReadOnlyList<BookFile> files)
    {
        bundleName = BundleName;
        bookTitle = BookTitle;
        files = Files;
    }
}