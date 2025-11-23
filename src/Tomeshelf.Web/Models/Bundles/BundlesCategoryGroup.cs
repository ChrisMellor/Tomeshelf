using System.Collections.Generic;

namespace Tomeshelf.Web.Models.Bundles;

/// <summary>
///     Represents a grouped collection of bundles by category/stamp.
/// </summary>
public sealed record BundlesCategoryGroup
{
    /// <summary>
    ///     Represents a grouped collection of bundles by category/stamp.
    /// </summary>
    /// <param name="category">Display category label.</param>
    /// <param name="bundles">Bundles belonging to the category.</param>
    public BundlesCategoryGroup(string category, IReadOnlyList<BundleViewModel> bundles)
    {
        Category = category;
        Bundles = bundles;
    }

    /// <summary>Display category label.</summary>
    public string Category { get; init; }

    /// <summary>Bundles belonging to the category.</summary>
    public IReadOnlyList<BundleViewModel> Bundles { get; init; }

    public void Deconstruct(out string category, out IReadOnlyList<BundleViewModel> bundles)
    {
        category = Category;
        bundles = Bundles;
    }
}