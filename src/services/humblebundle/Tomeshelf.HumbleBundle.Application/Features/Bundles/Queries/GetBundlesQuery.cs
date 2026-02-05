using System.Collections.Generic;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.HumbleBundle.Application.HumbleBundle;

namespace Tomeshelf.HumbleBundle.Application.Features.Bundles.Queries;

public sealed record GetBundlesQuery(bool IncludeExpired) : IQuery<IReadOnlyList<BundleDto>>;