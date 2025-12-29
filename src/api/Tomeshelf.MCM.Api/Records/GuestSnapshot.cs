using System.Collections.Generic;

namespace Tomeshelf.Mcm.Api.Records;

/// <summary>
///     Represents an immutable snapshot of the current guest list, including the total number of guests and their details.
/// </summary>
/// <param name="Total">The total number of guests included in the snapshot.</param>
/// <param name="Items">A read-only list containing the details of each guest in the snapshot. Cannot be null.</param>
public sealed record GuestSnapshot(int Total, IReadOnlyList<GuestListItem> Items);