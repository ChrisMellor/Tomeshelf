using System;

namespace Tomeshelf.Infrastructure.Domains.Bundles.Records;

/// <summary>
///     Summary of a bundle ingest operation.
/// </summary>
public sealed record BundleIngestResult
{
    /// <summary>
    ///     Summary of a bundle ingest operation.
    /// </summary>
    /// <param name="created">Number of bundles newly created.</param>
    /// <param name="updated">Number of bundles updated.</param>
    /// <param name="unchanged">Number of bundles that were unchanged.</param>
    /// <param name="processed">Total number of bundles processed.</param>
    /// <param name="observedAtUtc">Timestamp representing the newest observation in the batch.</param>
    public BundleIngestResult(int created, int updated, int unchanged, int processed, DateTimeOffset observedAtUtc)
    {
        Created = created;
        Updated = updated;
        Unchanged = unchanged;
        Processed = processed;
        ObservedAtUtc = observedAtUtc;
    }

    /// <summary>Number of bundles newly created.</summary>
    public int Created { get; init; }

    /// <summary>Number of bundles updated.</summary>
    public int Updated { get; init; }

    /// <summary>Number of bundles that were unchanged.</summary>
    public int Unchanged { get; init; }

    /// <summary>Total number of bundles processed.</summary>
    public int Processed { get; init; }

    /// <summary>Timestamp representing the newest observation in the batch.</summary>
    public DateTimeOffset ObservedAtUtc { get; init; }

    public void Deconstruct(out int created, out int updated, out int unchanged, out int processed, out DateTimeOffset observedAtUtc)
    {
        created = Created;
        updated = Updated;
        unchanged = Unchanged;
        processed = Processed;
        observedAtUtc = ObservedAtUtc;
    }
}