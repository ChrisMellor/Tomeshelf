using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Overview.Models;

namespace Tomeshelf.Fitbit.Application.Features.Overview.Queries;

public sealed class GetFitbitOverviewQueryHandler : IQueryHandler<GetFitbitOverviewQuery, FitbitOverviewDto>
{
    private readonly IFitbitOverviewService _overviewService;

    public GetFitbitOverviewQueryHandler(IFitbitOverviewService overviewService)
    {
        _overviewService = overviewService;
    }

    public Task<FitbitOverviewDto> Handle(GetFitbitOverviewQuery query, CancellationToken cancellationToken)
    {
        return _overviewService.GetOverviewAsync(query.Date, query.ForceRefresh, cancellationToken);
    }
}