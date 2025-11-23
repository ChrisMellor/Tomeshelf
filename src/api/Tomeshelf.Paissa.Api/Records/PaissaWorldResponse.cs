using System;
using System.Collections.Generic;

namespace Tomeshelf.Paissa.Api.Records;

/// <summary>
///     Response payload representing housing plots grouped by district and size.
/// </summary>
public sealed record PaissaWorldResponse
{
    /// <summary>
    ///     Response payload representing housing plots grouped by district and size.
    /// </summary>
    /// <param name="worldId">Identifier of the world.</param>
    /// <param name="worldName">Display name of the world.</param>
    /// <param name="retrievedAtUtc">Timestamp when the data was retrieved from PaissaDB.</param>
    /// <param name="districts">Districts that currently have lots accepting entries.</param>
    public PaissaWorldResponse(int worldId, string worldName, DateTimeOffset retrievedAtUtc, IReadOnlyList<PaissaDistrictResponse> districts)
    {
        WorldId = worldId;
        WorldName = worldName;
        RetrievedAtUtc = retrievedAtUtc;
        Districts = districts;
    }

    /// <summary>Identifier of the world.</summary>
    public int WorldId { get; init; }

    /// <summary>Display name of the world.</summary>
    public string WorldName { get; init; }

    /// <summary>Timestamp when the data was retrieved from PaissaDB.</summary>
    public DateTimeOffset RetrievedAtUtc { get; init; }

    /// <summary>Districts that currently have lots accepting entries.</summary>
    public IReadOnlyList<PaissaDistrictResponse> Districts { get; init; }

    public void Deconstruct(out int worldId, out string worldName, out DateTimeOffset retrievedAtUtc, out IReadOnlyList<PaissaDistrictResponse> districts)
    {
        worldId = WorldId;
        worldName = WorldName;
        retrievedAtUtc = RetrievedAtUtc;
        districts = Districts;
    }
}