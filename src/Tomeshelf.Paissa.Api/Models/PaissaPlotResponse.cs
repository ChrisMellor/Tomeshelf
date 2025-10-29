using System;

namespace Tomeshelf.Paissa.Api.Models;

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