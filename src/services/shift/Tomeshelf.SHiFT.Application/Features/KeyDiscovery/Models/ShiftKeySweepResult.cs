using System;
using System.Collections.Generic;

namespace Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Models;

public sealed record ShiftKeySweepResult(
    DateTimeOffset SinceUtc,
    DateTimeOffset ScannedAtUtc,
    ShiftKeySweepSummary Summary,
    IReadOnlyList<ShiftKeySweepItem> Items);
