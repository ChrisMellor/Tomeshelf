using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.HumbleBundle.Application.Features.Bundles.Models;

namespace Tomeshelf.HumbleBundle.Application.Features.Bundles.Commands;

public sealed record RefreshBundlesCommand() : ICommand<BundleIngestResult>;
