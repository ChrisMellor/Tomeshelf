using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.HumbleBundle.Application.Abstractions.Persistence;
using Tomeshelf.HumbleBundle.Application.HumbleBundle;

namespace Tomeshelf.HumbleBundle.Application.Features.Bundles.Queries;

public sealed class GetBundlesQueryHandler : IQueryHandler<GetBundlesQuery, IReadOnlyList<BundleDto>>
{
    private readonly IBundleQueries _queries;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetBundlesQueryHandler" /> class.
    /// </summary>
    /// <param name="queries">The queries.</param>
    public GetBundlesQueryHandler(IBundleQueries queries)
    {
        _queries = queries;
    }

    /// <summary>
    ///     Handles.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public Task<IReadOnlyList<BundleDto>> Handle(GetBundlesQuery query, CancellationToken cancellationToken)
    {
        return _queries.GetBundlesAsync(query.IncludeExpired, cancellationToken);
    }
}