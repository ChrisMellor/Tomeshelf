using System.Collections.Generic;
using Tomeshelf.Web.Models.ComicCon;

namespace Tomeshelf.Web.Models;

public sealed record GuestsByCityResult
{
    public GuestsByCityResult(IReadOnlyList<GuestsGroupModel> groups, int total)
    {
        Groups = groups;
        Total = total;
    }

    public IReadOnlyList<GuestsGroupModel> Groups { get; init; }

    public int Total { get; init; }

    public void Deconstruct(out IReadOnlyList<GuestsGroupModel> groups, out int total)
    {
        groups = Groups;
        total = Total;
    }
}