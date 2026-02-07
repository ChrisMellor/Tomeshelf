using Microsoft.Extensions.Logging.Abstractions;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.HumbleBundle.Api.Controllers;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Commands;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Models;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Queries;
using Tomeshelf.HumbleBundle.Application.HumbleBundle;

namespace Tomeshelf.HumbleBundle.Api.Tests.TestUtilities;

public static class BundlesControllerTestHarness
{
    public static BundlesController CreateController(IQueryHandler<GetBundlesQuery, IReadOnlyList<BundleDto>> queryHandler, ICommandHandler<RefreshBundlesCommand, BundleIngestResult> refreshHandler)
    {
        return new BundlesController(queryHandler, refreshHandler, NullLogger<BundlesController>.Instance);
    }

    public static BundleDto CreateDto(DateTimeOffset? endsAt)
    {
        var now = DateTimeOffset.UtcNow;

        return new BundleDto("bundle-one", "books", "bundle", "Bundle One", "One", "https://example.com/bundle-one", "tile", "logo", "hero", "desc", now.AddDays(-1), endsAt, now.AddDays(-3), now.AddDays(-2), now.AddMinutes(-10), now.AddMinutes(-5));
    }
}
