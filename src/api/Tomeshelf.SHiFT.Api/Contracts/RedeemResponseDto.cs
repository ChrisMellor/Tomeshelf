using System.Collections.Generic;
using Tomeshelf.Application.Shared.Contracts.SHiFT;

namespace Tomeshelf.SHiFT.Api.Contracts;

/// <summary>
///     Represents the result of a redemption operation, including a summary and the details of each individual redemption
///     attempt.
/// </summary>
/// <param name="Summary">
///     A summary of the overall redemption operation, including aggregate information such as totals or status.
/// </param>
/// <param name="Results">
///     A read-only list containing the results of each individual redemption attempt. Each item provides details about a
///     specific redemption.
/// </param>
public sealed record RedeemResponseDto(RedeemSummaryDto Summary, IReadOnlyList<RedeemResult> Results);