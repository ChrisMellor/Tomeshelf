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

    public GetBundlesQueryHandler(IBundleQueries queries)
    {
        _queries = queries;
    }

    public Task<IReadOnlyList<BundleDto>> Handle(GetBundlesQuery query, CancellationToken cancellationToken)
    {
        return _queries.GetBundlesAsync(query.IncludeExpired, cancellationToken);
    }
}
