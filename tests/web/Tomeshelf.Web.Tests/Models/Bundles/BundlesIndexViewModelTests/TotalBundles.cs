using System;
using System.Collections.Generic;
using Shouldly;
using Tomeshelf.Web.Models.Bundles;

namespace Tomeshelf.Web.Tests.Models.Bundles.BundlesIndexViewModelTests;

public class TotalBundles
{
    /// <summary>
    ///     Onlys counts active bundles when the include expired false.
    /// </summary>
    [Fact]
    public void WhenIncludeExpiredFalse_OnlyCountsActiveBundles()
    {
        // Arrange
        var model = new BundlesIndexViewModel
        {
            ActiveBundles = new List<BundlesCategoryGroup>
            {
                new("Games", new List<BundleViewModel>
                {
                    new(),
                    new()
                }),
                new("Books", new List<BundleViewModel> { new() })
            },
            ExpiredBundles = new List<BundleViewModel>
            {
                new(),
                new()
            },
            IncludeExpired = false,
            DataTimestampUtc = DateTimeOffset.UtcNow
        };

        // Act
        var total = model.TotalBundles;

        // Assert
        total.ShouldBe(3);
    }

    /// <summary>
    ///     Counts active and expired bundles when the include expired true.
    /// </summary>
    [Fact]
    public void WhenIncludeExpiredTrue_CountsActiveAndExpiredBundles()
    {
        // Arrange
        var model = new BundlesIndexViewModel
        {
            ActiveBundles = new List<BundlesCategoryGroup>
            {
                new("Games", new List<BundleViewModel>
                {
                    new(),
                    new()
                })
            },
            ExpiredBundles = new List<BundleViewModel>
            {
                new(),
                new(),
                new()
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