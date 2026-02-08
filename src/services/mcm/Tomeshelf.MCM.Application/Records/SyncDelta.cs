namespace Tomeshelf.MCM.Application.Records;

/// <summary>
///     Represents a summary of changes resulting from a synchronization operation, including counts of items added,
///     updated, removed, and the total number of items processed.
/// </summary>
/// <param name="Added">The number of items that were newly added during the synchronization.</param>
/// <param name="Updated">The number of items that were updated during the synchronization.</param>
/// <param name="Removed">The number of items that were removed during the synchronization.</param>
/// <param name="Total">
///     The total number of items processed during the synchronization, including added, updated, and
///     removed items.
/// </param>
public sealed record SyncDelta(int Added, int Updated, int Removed, int Total);