using System.Collections.Generic;
using Tomeshelf.Web.Models.ComicCon;

namespace Tomeshelf.Web.Models;

/// <summary>
///     Represents the result of grouping guests by event, including the grouped guest data and the total number of guests.
/// </summary>
/// <param name="Groups">
///     A read-only list of guest groups, where each group contains guests associated with a specific event or category.
///     Cannot be null.
/// </param>
/// <param name="Total">The total number of guests included across all groups. Must be greater than or equal to zero.</param>
public sealed record GuestsByEventResult(IReadOnlyList<GuestsGroupModel> Groups, int Total);