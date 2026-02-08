using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Tomeshelf.FileUploader.Infrastructure.Upload;

public sealed class BundleFileOrganiser
{
    public IReadOnlyList<BookPlan> BuildPlan(string rootDirectory)
    {
        var files = Directory.EnumerateFiles(rootDirectory, "*.*", SearchOption.AllDirectories)
                             .Select(fileName => new FileInfo(fileName))
                             .GroupBy(f => Path.GetFileNameWithoutExtension(f.Name.Replace("_supplement", string.Empty, StringComparison.OrdinalIgnoreCase)), StringComparer.OrdinalIgnoreCase)
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

            foreach (var file in fileGroup)
            {
                var isSupplement = file.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase) && file.Name.Contains("supplement", StringComparison.OrdinalIgnoreCase);
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

    private static string SanitizeForPath(string s)
    {
        s = s.Replace(":", " -", StringComparison.Ordinal)
             .Replace("/", " -", StringComparison.Ordinal)
             .ReplaceLineEndings(string.Empty);

        s = Path.GetInvalidFileNameChars()
                .Aggregate(s, (current, c) => current.Replace(c, ' '));

        return s.Trim();
    }

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