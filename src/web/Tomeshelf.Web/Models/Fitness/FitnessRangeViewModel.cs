using System.Collections.Generic;

namespace Tomeshelf.Web.Models.Fitness;

public sealed class FitnessRangeViewModel
{
    public string Title { get; init; }

    public string DateRangeLabel { get; init; }

    public IReadOnlyList<FitnessMetricSeriesViewModel> Metrics { get; init; } = new List<FitnessMetricSeriesViewModel>();

    public bool HasData { get; init; }
}
