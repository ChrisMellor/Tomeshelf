using System;

namespace Tomeshelf.Infrastructure.Bundles;

/// <summary>
///     Summary of a bundle ingest operation.
/// </summary>
/// <param name="Created">Number of bundles newly created.</param>
/// <param name="Updated">Number of bundles updated.</param>
/// <param name="Unchanged">Number of bundles that were unchanged.</param>
/// <param name="Processed">Total number of bundles processed.</param>
/// <param name="ObservedAtUtc">Timestamp representing the newest observation in the batch.</param>
public sealed record BundleIngestResult(int Created, int Updated, int Unchanged, int Processed, DateTimeOffset ObservedAtUtc);