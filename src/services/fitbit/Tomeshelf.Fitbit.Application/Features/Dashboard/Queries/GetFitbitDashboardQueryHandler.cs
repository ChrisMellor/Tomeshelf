using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Fitbit.Application.Abstractions.Services;

namespace Tomeshelf.Fitbit.Application.Features.Dashboard.Queries;

public sealed class GetFitbitDashboardQueryHandler : IQueryHandler<GetFitbitDashboardQuery, FitbitDashboardDto>
{
    private readonly IFitbitDashboardService _dashboardService;

    public GetFitbitDashboardQueryHandler(IFitbitDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public Task<FitbitDashboardDto> Handle(GetFitbitDashboardQuery query, CancellationToken cancellationToken)
    {
        return _dashboardService.GetDashboardAsync(query.Date, query.ForceRefresh, cancellationToken);
    }
}