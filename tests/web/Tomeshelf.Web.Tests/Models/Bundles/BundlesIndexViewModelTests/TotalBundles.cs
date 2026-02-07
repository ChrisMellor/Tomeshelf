using System;
using System.Collections.Generic;
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
                new BundlesCategoryGroup("Games", new List<BundleViewModel>
                {
                    new BundleViewModel(),
                    new BundleViewModel()
                }),
                new BundlesCategoryGroup("Books", new List<BundleViewModel> { new BundleViewModel() })
            },
            ExpiredBundles = new List<BundleViewModel>
            {
                new BundleViewModel(),
                new BundleViewModel()
            },
            IncludeExpired = false,
            DataTimestampUtc = DateTimeOffset.UtcNow
        };

        // Act
        var total = model.TotalBundles;

        // Assert
        total.ShouldBe(3);
    }

    [Fact]
    public void WhenIncludeExpiredTrue_CountsActiveAndExpiredBundles()
    {
        // Arrange
        var model = new BundlesIndexViewModel
        {
            ActiveBundles = new List<BundlesCategoryGroup>
            {
                new BundlesCategoryGroup("Games", new List<BundleViewModel>
                {
                    new BundleViewModel(),
                    new BundleViewModel()
                })
            },
            ExpiredBundles = new List<BundleViewModel>
            {
                new BundleViewModel(),
                new BundleViewModel(),
                new BundleViewModel()
            },
            IncludeExpired = true,
            DataTimestampUtc = DateTimeOffset.UtcNow
        };

        // Act
        var total = model.TotalBundles;

        // Assert
        total.ShouldBe(5);
    }
}
