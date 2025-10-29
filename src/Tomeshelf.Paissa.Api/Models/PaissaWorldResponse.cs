using System;
using System.Collections.Generic;

namespace Tomeshelf.Paissa.Api.Models;

/// <summary>
///     Response payload representing housing plots grouped by district and size.
/// </summary>
/// <param name="WorldId">Identifier of the world.</param>
/// <param name="WorldName">Display name of the world.</param>
/// <param name="RetrievedAtUtc">Timestamp when the data was retrieved from PaissaDB.</param>
/// <param name="Districts">Districts that currently have lots accepting entries.</param>
public sealed record PaissaWorldResponse(int WorldId, string WorldName, DateTimeOffset RetrievedAtUtc, IReadOnlyList<PaissaDistrictResponse> Districts);

/// <summary>
///     District containing grouped plot information.
/// </summary>
/// <param name="Id">District identifier.</param>
/// <param name="Name">District name.</param>
/// <param name="Tabs">Groupings for small, medium and large plots.</param>
public sealed record PaissaDistrictResponse(int Id, string Name, IReadOnlyList<PaissaSizeGroupResponse> Tabs);

/// <summary>
///     Group of plots for a specific size category.
/// </summary>
/// <param name="Size">Display label for the size.</param>
/// <param name="SizeKey">Stable key for the size (small, medium, large).</param>
/// <param name="Plots">Plots within the size group.</param>
public sealed record PaissaSizeGroupResponse(string Size, string SizeKey, IReadOnlyList<PaissaPlotResponse> Plots);

/// <summary>
///     Plot information exposed to the web front-end.
/// </summary>
/// <param name="Ward">Ward number.</param>
/// <param name="Plot">Plot number.</param>
/// <param name="Price">Price in gil.</param>
/// <param name="Entries">Number of lottery entries.</param>
/// <param name="LastUpdatedUtc">Last update timestamp from PaissaDB.</param>
/// <param name="AllowsPersonal">Indicates whether personal buyers may bid.</param>
/// <param name="AllowsFreeCompany">Indicates whether free companies may bid.</param>
/// <param name="IsEligibilityUnknown">True when buyer eligibility is unknown.</param>
public sealed record PaissaPlotResponse(int Ward, int Plot, long Price, int Entries, DateTimeOffset LastUpdatedUtc, bool AllowsPersonal, bool AllowsFreeCompany, bool IsEligibilityUnknown);