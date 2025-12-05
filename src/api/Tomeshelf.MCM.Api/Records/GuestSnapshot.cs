using System.Collections.Generic;

namespace Tomeshelf.MCM.Api.Records;

/// <summary>
///     Represents an immutable snapshot of guest records, including the total count and a read-only list of guests.
/// </summary>
/// <param name="Total">The total number of guest records included in the snapshot.</param>
/// <param name="Items">A read-only list containing the guest records represented by this snapshot. Cannot be null.</param>
public sealed record GuestSnapshot(int Total, IReadOnlyList<McmGuestRecord> Items);