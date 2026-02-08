using System.Collections.Generic;
using Tomeshelf.SHiFT.Application.Features.Redemption.Redeem;

namespace Tomeshelf.SHiFT.Api.Contracts;

/// <summary>
///     Represents a single SHiFT key found during a sweep and the redemption results for that key.
/// </summary>
/// <param name="Code">The SHiFT code that was redeemed.</param>
/// <param name="Sources">Sources that reported the code.</param>
/// <param name="Summary">Summary of redemption attempts for this code.</param>
/// <param name="Results">Detailed redemption results for each configured account.</param>
public sealed record ShiftKeySweepItemDto(string Code, IReadOnlyList<string> Sources, RedeemSummaryDto Summary, IReadOnlyList<RedeemResult> Results);