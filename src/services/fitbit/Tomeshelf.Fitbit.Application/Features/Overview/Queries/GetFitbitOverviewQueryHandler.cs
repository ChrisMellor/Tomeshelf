using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Overview.Models;

namespace Tomeshelf.Fitbit.Application.Features.Overview.Queries;

public sealed class GetFitbitOverviewQueryHandler : IQueryHandler<GetFitbitOverviewQuery, FitbitOverviewDto>
{
    private readonly IFitbitOverviewService _overviewService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetFitbitOverviewQueryHandler" /> class.
    /// </summary>
    /// <param name="overviewService">The overview service.</param>
    public GetFitbitOverviewQueryHandler(IFitbitOverviewService overviewService)
    {
        _overviewService = overviewService;
    }

    /// <summary>
    ///     Handles.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public Task<FitbitOverviewDto> Handle(GetFitbitOverviewQuery query, CancellationToken cancellationToken)
    {
        return _overviewService.GetOverviewAsync(query.Date, query.ForceRefresh, cancellationToken);
    }
}