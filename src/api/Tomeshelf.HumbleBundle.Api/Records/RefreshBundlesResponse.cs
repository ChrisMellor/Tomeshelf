using System;

namespace Tomeshelf.HumbleBundle.Api.Records;

/// <summary>
///     Summary returned after invoking the refresh endpoint.
/// </summary>
public sealed record RefreshBundlesResponse
{
    /// <summary>
    ///     Summary returned after invoking the refresh endpoint.
    /// </summary>
    /// <param name="created">Bundles created during the ingest.</param>
    /// <param name="updated">Bundles updated during the ingest.</param>
    /// <param name="unchanged">Bundles left unchanged.</param>
    /// <param name="processed">Total bundles processed.</param>
    /// <param name="observedAtUtc">Observation timestamp supplied by the ingest.</param>
    public RefreshBundlesResponse(int created, int updated, int unchanged, int processed, DateTimeOffset observedAtUtc)
    {
        Created = created;
        Updated = updated;
        Unchanged = unchanged;
        Processed = processed;
        ObservedAtUtc = observedAtUtc;
    }

    /// <summary>Bundles created during the ingest.</summary>
    public int Created { get; init; }

    /// <summary>Bundles updated during the ingest.</summary>
    public int Updated { get; init; }

    /// <summary>Bundles left unchanged.</summary>
    public int Unchanged { get; init; }

    /// <summary>Total bundles processed.</summary>
    public int Processed { get; init; }

    /// <summary>Observation timestamp supplied by the ingest.</summary>
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