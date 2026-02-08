using System;

namespace Tomeshelf.HumbleBundle.Application.Features.Bundles.Models;

public sealed record BundleIngestResult(int Created, int Updated, int Unchanged, int Processed, DateTimeOffset ObservedAtUtc);