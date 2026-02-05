using System.Collections.Generic;

namespace Tomeshelf.Paissa.Api.Contracts;

/// <summary>
///     District containing grouped plot information.
/// </summary>
/// <param name="Id">District identifier.</param>
/// <param name="Name">District name.</param>
/// <param name="Tabs">Groupings for small, medium and large plots.</param>
public sealed record PaissaDistrictResponse(int Id, string Name, IReadOnlyList<PaissaSizeGroupResponse> Tabs);