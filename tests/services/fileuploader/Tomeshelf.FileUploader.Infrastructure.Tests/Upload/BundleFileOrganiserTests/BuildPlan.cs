using Shouldly;
using Tomeshelf.FileUploader.Infrastructure.Upload;

namespace Tomeshelf.FileUploader.Infrastructure.Tests.Upload.BundleFileOrganiserTests;

public class BuildPlan
{
    [Fact]
    public void FallsBackToRootDirectoryName_WhenNoBundleFolderPresent()
    {
        // Arrange
        // Act
        var root = CreateTempDirectory();
        // Assert
        try
        {
            File.WriteAllText(Path.Combine(root, "PlainBook.txt"), "data");

            var organiser = new BundleFileOrganiser();

            var plans = organiser.BuildPlan(root);

            plans.ShouldHaveSingleItem();
            plans[0]
               .BundleName
               .ShouldBe(new DirectoryInfo(root).Name);
        }
        finally
        {
            TryDeleteDirectory(root);
        }
    }

    [Fact]
    public void UsesBundleDirectoryAndSupplementNaming()
    {
        // Arrange
        // Act
        var root = CreateTempDirectory();
        // Assert
        try
        {
            var bundleDir = Path.Combine(root, "Great Bundle by Authors");
            Directory.CreateDirectory(bundleDir);
            File.WriteAllText(Path.Combine(bundleDir, "BookOne.pdf"), "not a pdf");
            File.WriteAllBytes(Path.Combine(bundleDir, "BookOne_supplement.zip"), new byte[] { 1, 2, 3 });

            var organiser = new BundleFileOrganiser();

            var plans = organiser.BuildPlan(root);

            plans.ShouldHaveSingleItem();
            var plan = plans[0];
            plan.BundleName.ShouldBe("Great Bundle by Authors");
            plan.BookTitle.ShouldBe("Unknown Title");
            plan.Files
                .Select(file => file.TargetFileName)
                .ShouldBe(new[] { "Unknown Title.pdf", "Unknown Title - Supplement.zip" });
        }
        finally
        {
            TryDeleteDirectory(root);
        }
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "tomeshelf-tests", Guid.NewGuid()
                                                                           .ToString("N"));
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
        catch { }
    }
}