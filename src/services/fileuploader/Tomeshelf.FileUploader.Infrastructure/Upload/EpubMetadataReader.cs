using System;
using System.IO.Compression;
using System.Xml.Linq;

namespace Tomeshelf.FileUploader.Infrastructure.Upload;

internal static class EpubMetadataReader
{
    public static DocumentMetadata GetMetadata(string path)
    {
        using var zip = ZipFile.OpenRead(path);

        var containerEntry = zip.GetEntry("META-INF/container.xml") ?? throw new InvalidOperationException("container.xml not found (not a valid EPUB?)");
        using var containerStream = containerEntry.Open();
        var container = XDocument.Load(containerStream);

        var cns = (XNamespace)"urn:oasis:names:tc:opendocument:xmlns:container";
        var opfPath = container.Root
                              ?.Element(cns + "rootfiles")
                              ?.Element(cns + "rootfile")
                              ?.Attribute("full-path")
                              ?.Value ??
                      throw new InvalidOperationException("OPF path not found in container.xml");

        var opfEntry = zip.GetEntry(opfPath) ?? throw new InvalidOperationException($"OPF file '{opfPath}' not found in EPUB.");
        using var opfStream = opfEntry.Open();
        var opf = XDocument.Load(opfStream);

        var opfNs = (XNamespace)"http://www.idpf.org/2007/opf";
        var dc = (XNamespace)"http://purl.org/dc/elements/1.1/";

        var metadata = opf.Root?.Element(opfNs + "metadata") ?? throw new InvalidOperationException("metadata element not found in OPF.");

        var documentMetadata = new DocumentMetadata
        {
            Title = metadata.Element(dc + "title")
                           ?.Value
                           ?.Trim()
        };

        return documentMetadata;
    }
}