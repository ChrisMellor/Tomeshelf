using System;
using System.Collections.Generic;
using System.Linq;

namespace Tomeshelf.Web.Models.Bundles;

/// <summary>
///     View model for the bundles index page.
/// </summary>
public sealed class BundlesIndexViewModel
{
    public required IReadOnlyList<BundlesCategoryGroup> ActiveBundles { get; init; }
    public required IReadOnlyList<BundleViewModel> ExpiredBundles { get; init; }
    public bool IncludeExpired { get; init; }
    public DateTimeOffset DataTimestampUtc { get; init; }

    public int TotalBundles => (ActiveBundles?.Sum(group => group.Bundles.Count) ?? 0) +
                               (IncludeExpired ? ExpiredBundles?.Count ?? 0 : 0);
}

/// <summary>
///     Represents a grouped collection of bundles by category/stamp.
/// </summary>
/// <param name="Category">Display category label.</param>
/// <param name="Bundles">Bundles belonging to the category.</param>
public sealed record BundlesCategoryGroup(string Category, IReadOnlyList<BundleViewModel> Bundles);