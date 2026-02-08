using Bogus;
using Shouldly;
using Tomeshelf.FileUploader.Application.Features.Uploads.Models;

namespace Tomeshelf.FileUploader.Application.Tests.Features.Uploads.Models.BundleUploadResultTests;

public class FromBooks
{
    [Fact]
    public void DistinctBundles_AreCaseInsensitive()
    {
        // Arrange
        var faker = new Faker();
        var uploadedAt = faker.Date.RecentOffset();
        var bundleName = faker.Commerce.ProductName();
        var books = new List<BookUploadResult>
        {
            new BookUploadResult(bundleName, faker.Commerce.ProductName(), 2, 1),
            new BookUploadResult(bundleName.ToLowerInvariant(), faker.Commerce.ProductName(), 1, 0),
            new BookUploadResult(faker.Commerce.ProductName(), faker.Commerce.ProductName(), 3, 2)
        };

        // Act
        var result = BundleUploadResult.FromBooks(books, uploadedAt);

        // Assert
        result.BundlesProcessed.ShouldBe(2);
        result.BooksProcessed.ShouldBe(3);
        result.FilesUploaded.ShouldBe(6);
        result.FilesSkipped.ShouldBe(3);
    }

    [Fact]
    public void WithNoBooks_ReturnsZeroCounts()
    {
        // Arrange
        var faker = new Faker();
        var uploadedAt = faker.Date.RecentOffset();

        // Act
        var result = BundleUploadResult.FromBooks(Array.Empty<BookUploadResult>(), uploadedAt);

        // Assert
        result.UploadedAtUtc.ShouldBe(uploadedAt);
        result.BundlesProcessed.ShouldBe(0);
        result.BooksProcessed.ShouldBe(0);
        result.FilesUploaded.ShouldBe(0);
        result.FilesSkipped.ShouldBe(0);
        result.Books.ShouldBeEmpty();
    }
}