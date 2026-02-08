using System;

namespace Tomeshelf.MCM.Application.Contracts;

/// <summary>
///     Represents the result of a guest synchronization operation, including event details and counts of changes applied.
/// </summary>
/// <param name="Status">The status of the synchronization operation, such as 'Success' or 'Failed'.</param>
/// <param name="Added">The number of guest records that were added during the synchronization.</param>
/// <param name="Updated">The number of guest records that were updated during the synchronization.</param>
/// <param name="Removed">The number of guest records that were removed during the synchronization.</param>
/// <param name="Total">The total number of guest records after the synchronization is complete.</param>
/// <param name="RanAtUtc">
///     The date and time, in Coordinated Universal Time (UTC), when the synchronization operation was
///     executed.
/// </param>
public sealed record GuestSyncResultDto(string Status, int Added, int Updated, int Removed, int Total, DateTimeOffset RanAtUtc);