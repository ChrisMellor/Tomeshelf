namespace Tomeshelf.Fitbit.Application.Features.Overview.Models;

public sealed record FitbitOverviewDto
{
    public required FitbitDashboardDto Daily { get; init; }

    public required FitbitOverviewRangeDto Last7Days { get; init; }

    public required FitbitOverviewRangeDto Last30Days { get; init; }
}