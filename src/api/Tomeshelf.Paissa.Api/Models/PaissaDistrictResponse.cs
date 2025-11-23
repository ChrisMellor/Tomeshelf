using System.Collections.Generic;

namespace Tomeshelf.Paissa.Api.Models;

/// <summary>
///     District containing grouped plot information.
/// </summary>
public sealed record PaissaDistrictResponse
{
    /// <summary>
    ///     District containing grouped plot information.
    /// </summary>
    /// <param name="id">District identifier.</param>
    /// <param name="name">District name.</param>
    /// <param name="tabs">Groupings for small, medium and large plots.</param>
    public PaissaDistrictResponse(int id, string name, IReadOnlyList<PaissaSizeGroupResponse> tabs)
    {
        Id = id;
        Name = name;
        Tabs = tabs;
    }

    /// <summary>District identifier.</summary>
    public int Id { get; init; }

    /// <summary>District name.</summary>
    public string Name { get; init; }

    /// <summary>Groupings for small, medium and large plots.</summary>
    public IReadOnlyList<PaissaSizeGroupResponse> Tabs { get; init; }

    public void Deconstruct(out int id, out string name, out IReadOnlyList<PaissaSizeGroupResponse> tabs)
    {
        id = Id;
        name = Name;
        tabs = Tabs;
    }
}