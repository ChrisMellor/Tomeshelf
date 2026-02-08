using System.Collections.Generic;

namespace Tomeshelf.Paissa.Application.Features.Housing.Dtos;

/// <summary>
///     District containing grouped plot information.
/// </summary>
/// <param name="Id">District identifier.</param>
/// <param name="Name">District name.</param>
/// <param name="SizeGroups">Groupings for small, medium, and large plots.</param>
public sealed record PaissaDistrictSummaryDto(int Id, string Name, IReadOnlyList<PaissaSizeGroupSummaryDto> SizeGroups);