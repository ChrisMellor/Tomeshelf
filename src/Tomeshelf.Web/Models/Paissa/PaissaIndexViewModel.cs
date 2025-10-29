using System;
using System.Collections.Generic;

namespace Tomeshelf.Web.Models.Paissa;

public sealed class PaissaIndexViewModel
{
    public required string WorldName { get; init; }

    public required int WorldId { get; init; }

    public required DateTimeOffset RetrievedAtUtc { get; init; }

    public required IReadOnlyList<PaissaDistrictModel> Districts { get; init; }

    public int TotalPlotCount { get; init; }
}