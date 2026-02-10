using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Tomeshelf.FileUploader.Infrastructure.Upload;

public sealed class BundleFileOrganiser
{
    /// <summary>
    ///     Builds the plan.
    /// </summary>
    /// <param name="rootDirectory">The root directory.</param>
    /// <returns>The result of the operation.</returns>
    public IReadOnlyList<BookPlan> BuildPlan(string rootDirectory)
    {
        var files = Directory.EnumerateFiles(rootDirectory, "*.*", SearchOption.AllDirectories)
                              .Select(fileName => new FileInfo(fileName))
                              .GroupBy(f => Path.GetFileNameWithoutExtension(f.Name.Replace("_supplement", string.Empty, StringComparison.OrdinalIgnoreCase)), StringComparer.OrdinalIgnoreCase)
                              .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                              .ToList();

        var plans = new List<BookPlan>(files.Count);

        foreach (var fileGroup in files)
        {
            var bundleName = GetBundleName(fileGroup.First()) ?? new DirectoryInfo(rootDirectory).Name;
            bundleName = SanitizeForPath(string.IsNullOrWhiteSpace(bundleName)
                                             ? "Unknown Bundle"
                                             : bundleName);

            var bookTitle = TryGetTitle(fileGroup) ?? "Unknown Title";
            bookTitle = SanitizeForPath(bookTitle);

            var bookFiles = new List<BookFile>();

            foreach (var file in OrderBookFiles(fileGroup))
            {
                var isSupplement = IsSupplementFile(file);
                var targetBase = isSupplement
                    ? $"{bookTitle} - Supplement"
                    : bookTitle;
                var destName = $"{targetBase}{file.Extension}";

                bookFiles.Add(new BookFile(file.FullName, destName, file.Length));
            }

            plans.Add(new BookPlan(bundleName, bookTitle, bookFiles));
        }

        return plans;
    }

    /// <summary>
    ///     Determines whether the specified file is a supplement file.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns>True if the condition is met; otherwise, false.</returns>
    private static bool IsSupplementFile(FileInfo file)
    {
        return file.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase) &&
               file.Name.Contains("supplement", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Orders the book files.
    /// </summary>
    /// <param name="files">The files.</param>
    /// <returns>The result of the operation.</returns>
    private static IEnumerable<FileInfo> OrderBookFiles(IEnumerable<FileInfo> files)
    {
        return files.OrderBy(file => IsSupplementFile(file) ? 1 : 0)
                    .ThenBy(file => file.Extension, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(file => file.Name, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Gets the bundle name.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns>The result of the operation.</returns>
    private static string? GetBundleName(FileInfo file)
    {
        var directory = file.Directory;
        while (directory is not null)
        {
            if (directory.Name.Contains(" by ", StringComparison.OrdinalIgnoreCase))
            {
                return directory.Name;
            }

            directory = directory.Parent;
        }

        return null;
    }

    /// <summary>
    ///     Gets the document metadata.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns>The result of the operation.</returns>
    private static DocumentMetadata GetDocumentMetadata(FileInfo file)
    {
        try
        {
            var documentMetadata = file.Extension.ToLower(CultureInfo.InvariantCulture) switch
            {
                ".pdf" => PdfMetadataReader.GetMetadata(file.FullName),
                ".epub" => EpubMetadataReader.GetMetadata(file.FullName),
                ".mobi" => MobiMetadataReader.GetMetadata(file.FullName),
                _ => new DocumentMetadata()
            };

            return documentMetadata;
        }
        catch
        {
            return new DocumentMetadata();
        }
    }

    /// <summary>
    ///     Sanitizes the for path.
    /// </summary>
    /// <param name="s">The s.</param>
    /// <returns>The resulting string.</returns>
    private static string SanitizeForPath(string s)
    {
        s = s.Replace(":", " -", StringComparison.Ordinal)
             .Replace("/", " -", StringComparison.Ordinal)
             .ReplaceLineEndings(string.Empty);

        s = Path.GetInvalidFileNameChars()
                .Aggregate(s, (current, c) => current.Replace(c, ' '));

        return s.Trim();
    }

    /// <summary>
    ///     Attempts to get a title.
    /// </summary>
    /// <param name="files">The files.</param>
    /// <returns>The result of the operation.</returns>
    private static string? TryGetTitle(IGrouping<string, FileInfo> files)
    {
        foreach (var ext in new[] { ".epub", ".pdf", ".mobi" })
        {
            var fileInfo = files.FirstOrDefault(x => x.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase));
            if (fileInfo is null)
            {
                continue;
            }

            var documentMetadata = GetDocumentMetadata(fileInfo);
            if (!string.IsNullOrWhiteSpace(documentMetadata?.Title))
            {
                return documentMetadata.Title;
            }
        }

        foreach (var fileInfo in files)
        {
            var documentMetadata = GetDocumentMetadata(fileInfo);
            if (!string.IsNullOrWhiteSpace(documentMetadata?.Title))
            {
                return documentMetadata.Title;
            }
        }

        return null;
    }
}
