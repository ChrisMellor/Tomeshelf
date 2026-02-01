using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Authorization.Models;

namespace Tomeshelf.Fitbit.Application.Features.Authorization.Queries;

public sealed class GetFitbitAuthorizationStatusQueryHandler : IQueryHandler<GetFitbitAuthorizationStatusQuery, FitbitAuthorizationStatus>
{
    private readonly IFitbitTokenCache _tokenCache;

    public GetFitbitAuthorizationStatusQueryHandler(IFitbitTokenCache tokenCache)
    {
        _tokenCache = tokenCache;
    }

    public Task<FitbitAuthorizationStatus> Handle(GetFitbitAuthorizationStatusQuery query, CancellationToken cancellationToken)
    {
        var hasAccess = !string.IsNullOrWhiteSpace(_tokenCache.AccessToken);
        var hasRefresh = !string.IsNullOrWhiteSpace(_tokenCache.RefreshToken);

        return Task.FromResult(new FitbitAuthorizationStatus(hasAccess, hasRefresh));
    }
}
