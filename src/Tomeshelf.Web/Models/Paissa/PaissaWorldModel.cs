using System;
using System.Collections.Generic;

namespace Tomeshelf.Web.Models.Paissa;

public sealed record PaissaWorldModel
{
    public PaissaWorldModel(int worldId, string worldName, DateTimeOffset retrievedAtUtc, IReadOnlyList<PaissaDistrictModel> districts)
    {
        WorldId = worldId;
        WorldName = worldName;
        RetrievedAtUtc = retrievedAtUtc;
        Districts = districts;
    }

    public int WorldId { get; init; }

    public string WorldName { get; init; }

    public DateTimeOffset RetrievedAtUtc { get; init; }

    public IReadOnlyList<PaissaDistrictModel> Districts { get; init; }

    public void Deconstruct(out int worldId, out string worldName, out DateTimeOffset retrievedAtUtc, out IReadOnlyList<PaissaDistrictModel> districts)
    {
        worldId = WorldId;
        worldName = WorldName;
        retrievedAtUtc = RetrievedAtUtc;
        districts = Districts;
    }
}