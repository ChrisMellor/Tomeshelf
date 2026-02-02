using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Tomeshelf.FileUploader.Infrastructure.Upload;

namespace Tomeshelf.FileUploader.Infrastructure.Tests.Upload;

public class BundleFileOrganiserTests
{
    [Fact]
    public void BuildPlan_UsesBundleDirectoryAndSupplementNaming()
    {
        var root = CreateTempDirectory();
        try
        {
            var bundleDir = Path.Combine(root, "Great Bundle by Authors");
            Directory.CreateDirectory(bundleDir);
            File.WriteAllText(Path.Combine(bundleDir, "BookOne.pdf"), "not a pdf");
            File.WriteAllBytes(Path.Combine(bundleDir, "BookOne_supplement.zip"), new byte[] { 1, 2, 3 });

            var organiser = new BundleFileOrganiser();

            var plans = organiser.BuildPlan(root);

            plans.Should().HaveCount(1);
            var plan = plans[0];
            plan.BundleName.Should().Be("Great Bundle by Authors");
            plan.BookTitle.Should().Be("Unknown Title");
            plan.Files.Select(file => file.TargetFileName)
                .Should().BeEquivalentTo(new[]
                {
                    "Unknown Title.pdf",
                    "Unknown Title - Supplement.zip"
                });
        }
        finally
        {
            TryDeleteDirectory(root);
        }
    }

    [Fact]
    public void BuildPlan_FallsBackToRootDirectoryName_WhenNoBundleFolderPresent()
    {
        var root = CreateTempDirectory();
        try
        {
            File.WriteAllText(Path.Combine(root, "PlainBook.txt"), "data");

            var organiser = new BundleFileOrganiser();

            var plans = organiser.BuildPlan(root);

            plans.Should().HaveCount(1);
            plans[0].BundleName.Should().Be(new DirectoryInfo(root).Name);
        }
        finally
        {
            TryDeleteDirectory(root);
        }
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "tomeshelf-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);

        return path;
    }

    private static void TryDeleteDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        catch
        {
        }
    }
}
