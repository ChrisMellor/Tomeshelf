using System;
using System.Collections.Generic;

namespace Tomeshelf.SHiFT.Api.Contracts;

/// <summary>
///     Represents the response payload for a SHiFT key sweep.
/// </summary>
/// <param name="SinceUtc">The timestamp used as the lower bound for source scanning.</param>
/// <param name="ScannedAtUtc">The time the sweep completed.</param>
/// <param name="Summary">Aggregate summary of the sweep.</param>
/// <param name="Items">Per-key redemption results.</param>
public sealed record ShiftKeySweepResponseDto(
    DateTimeOffset SinceUtc,
    DateTimeOffset ScannedAtUtc,
    ShiftKeySweepSummaryDto Summary,
    IReadOnlyList<ShiftKeySweepItemDto> Items);
