namespace Tomeshelf.Web.Models.Fitness;

public sealed class FitbitOverviewModel
{
    public FitbitDashboardModel Daily { get; set; } = new FitbitDashboardModel();

    public FitbitOverviewRangeModel Last7Days { get; set; } = new FitbitOverviewRangeModel();

    public FitbitOverviewRangeModel Last30Days { get; set; } = new FitbitOverviewRangeModel();
}