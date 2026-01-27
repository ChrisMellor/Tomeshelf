using System.Collections.Generic;

namespace Tomeshelf.Web.Models.Fitness;

public sealed class FitbitOverviewRangeModel
{
    public int Days { get; set; }

    public List<FitbitOverviewDayModel> Items { get; set; } = new();
}
