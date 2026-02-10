using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Fitbit.Application.Abstractions.Services;

namespace Tomeshelf.Fitbit.Application.Features.Dashboard.Queries;

public sealed class GetFitbitDashboardQueryHandler : IQueryHandler<GetFitbitDashboardQuery, FitbitDashboardDto>
{
    private readonly IFitbitDashboardService _dashboardService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetFitbitDashboardQueryHandler" /> class.
    /// </summary>
    /// <param name="dashboardService">The dashboard service.</param>
    public GetFitbitDashboardQueryHandler(IFitbitDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    ///     Handles.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public Task<FitbitDashboardDto> Handle(GetFitbitDashboardQuery query, CancellationToken cancellationToken)
    {
        return _dashboardService.GetDashboardAsync(query.Date, query.ForceRefresh, cancellationToken);
    }
}