using System;
using System.Collections.Generic;
using FluentAssertions;
using Tomeshelf.Web.Models.Bundles;

namespace Tomeshelf.Web.Tests.Models.Bundles.BundlesIndexViewModelTests;

public class TotalBundles
{
    [Fact]
    public void WhenIncludeExpiredFalse_OnlyCountsActiveBundles()
    {
        // Arrange
        var model = new BundlesIndexViewModel
        {
            ActiveBundles = new List<BundlesCategoryGroup>
            {
                new("Games", new List<BundleViewModel> { new(), new() }),
                new("Books", new List<BundleViewModel> { new() })
            },
            ExpiredBundles = new List<BundleViewModel> { new(), new() },
            IncludeExpired = false,
            DataTimestampUtc = DateTimeOffset.UtcNow
        };

        // Act
        var total = model.TotalBundles;

        // Assert
        total.Should().Be(3);
    }

    [Fact]
    public void WhenIncludeExpiredTrue_CountsActiveAndExpiredBundles()
    {
        // Arrange
        var model = new BundlesIndexViewModel
        {
            ActiveBundles = new List<BundlesCategoryGroup>
            {
                new("Games", new List<BundleViewModel> { new(), new() })
            },
            ExpiredBundles = new List<BundleViewModel> { new(), new(), new() },
            IncludeExpired = true,
            DataTimestampUtc = DateTimeOffset.UtcNow
        };

        // Act
        var total = model.TotalBundles;

        // Assert
        total.Should().Be(5);
    }
}
