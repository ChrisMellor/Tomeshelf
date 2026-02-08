using System.Collections.Generic;

namespace Tomeshelf.Web.Models.Bundles;

/// <summary>
///     Represents a grouped collection of bundles by category/stamp.
/// </summary>
/// <param name="Category">Display category label.</param>
/// <param name="Bundles">Bundles belonging to the category.</param>
public sealed record BundlesCategoryGroup(string Category, IReadOnlyList<BundleViewModel> Bundles);