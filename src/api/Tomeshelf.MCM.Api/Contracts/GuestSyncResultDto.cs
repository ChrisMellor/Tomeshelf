using System;
using Tomeshelf.MCM.Api.Enums;

namespace Tomeshelf.MCM.Api.Contracts;

/// <summary>
///     Represents the result of a guest synchronization operation for a specific city, including status and counts of
///     added, updated, and removed guests.
/// </summary>
/// <param name="City">The city for which the guest synchronization was performed.</param>
/// <param name="Status">
///     The status of the synchronization operation. Typically, indicates success, failure, or other
///     relevant state.
/// </param>
/// <param name="Added">The number of guests that were added during the synchronization.</param>
/// <param name="Updated">The number of guests that were updated during the synchronization.</param>
/// <param name="Removed">The number of guests that were removed during the synchronization.</param>
/// <param name="Total">The total number of guests present after the synchronization.</param>
/// <param name="RanAtUtc">The date and time, in UTC, when the synchronization operation was executed.</param>
public sealed record GuestSyncResultDto(City City, string Status, int Added, int Updated, int Removed, int Total, DateTimeOffset RanAtUtc);