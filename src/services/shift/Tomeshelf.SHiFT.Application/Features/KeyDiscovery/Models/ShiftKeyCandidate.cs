using System;

namespace Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Models;

public sealed record ShiftKeyCandidate(string Code, string Source, DateTimeOffset? PublishedUtc);