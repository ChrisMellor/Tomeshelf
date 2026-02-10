using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Application.Features.Authorization.Models;

namespace Tomeshelf.Fitbit.Application.Features.Authorization.Queries;

public sealed class GetFitbitAuthorizationStatusQueryHandler : IQueryHandler<GetFitbitAuthorizationStatusQuery, FitbitAuthorizationStatus>
{
    private readonly IFitbitTokenCache _tokenCache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetFitbitAuthorizationStatusQueryHandler" /> class.
    /// </summary>
    /// <param name="tokenCache">The token cache.</param>
    public GetFitbitAuthorizationStatusQueryHandler(IFitbitTokenCache tokenCache)
    {
        _tokenCache = tokenCache;
    }

    /// <summary>
    ///     Handles.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public Task<FitbitAuthorizationStatus> Handle(GetFitbitAuthorizationStatusQuery query, CancellationToken cancellationToken)
    {
        var hasAccess = !string.IsNullOrWhiteSpace(_tokenCache.AccessToken);
        var hasRefresh = !string.IsNullOrWhiteSpace(_tokenCache.RefreshToken);

        return Task.FromResult(new FitbitAuthorizationStatus(hasAccess, hasRefresh));
    }
}