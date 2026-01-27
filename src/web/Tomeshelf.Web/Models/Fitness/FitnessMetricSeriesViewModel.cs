using System.Collections.Generic;
using System.Linq;

namespace Tomeshelf.Web.Models.Fitness;

public sealed class FitnessMetricSeriesViewModel
{
    public string Key { get; init; }

    public string Title { get; init; }

    public string Unit { get; init; }

    public IReadOnlyList<string> Labels { get; init; } = new List<string>();

    public IReadOnlyList<double?> Values { get; init; } = new List<double?>();

    public bool HasData => Values is not null && Values.Any(value => value.HasValue);
}
