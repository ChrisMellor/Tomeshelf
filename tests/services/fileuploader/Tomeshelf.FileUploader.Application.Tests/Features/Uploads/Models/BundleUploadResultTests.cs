using System;
using System.Collections.Generic;
using Tomeshelf.FileUploader.Application.Features.Uploads.Models;
using Xunit;

namespace Tomeshelf.FileUploader.Application.Tests.Features.Uploads.Models;

public class BundleUploadResultTests
{
    [Fact]
    public void FromBooks_WithNoBooks_ReturnsZeroCounts()
    {
        // Arrange
        var uploadedAt = DateTimeOffset.UtcNow;

        // Act
        var result = BundleUploadResult.FromBooks(Array.Empty<BookUploadResult>(), uploadedAt);

        // Assert
        Assert.Equal(uploadedAt, result.UploadedAtUtc);
        Assert.Equal(0, result.BundlesProcessed);
        Assert.Equal(0, result.BooksProcessed);
        Assert.Equal(0, result.FilesUploaded);
        Assert.Equal(0, result.FilesSkipped);
        Assert.Empty(result.Books);
    }

    [Fact]
    public void FromBooks_DistinctBundles_AreCaseInsensitive()
    {
        // Arrange
        var uploadedAt = DateTimeOffset.UtcNow;
        var books = new List<BookUploadResult>
        {
            new("Bundle One", "Book A", 2, 1),
            new("bundle one", "Book B", 1, 0),
            new("Bundle Two", "Book C", 3, 2)
        };

        // Act
        var result = BundleUploadResult.FromBooks(books, uploadedAt);

        // Assert
        Assert.Equal(2, result.BundlesProcessed);
        Assert.Equal(3, result.BooksProcessed);
        Assert.Equal(6, result.FilesUploaded);
        Assert.Equal(3, result.FilesSkipped);
    }
}
