namespace Tomeshelf.SHiFT.Api.Contracts;

/// <summary>
///     Represents a summary of a redemption operation, including the total number of attempts and the counts of successful
///     and failed redemptions.
/// </summary>
/// <param name="Total">The total number of redemption attempts included in the summary.</param>
/// <param name="Succeeded">The number of redemption attempts that succeeded.</param>
/// <param name="Failed">The number of redemption attempts that failed.</param>
public sealed record RedeemSummaryDto(int Total, int Succeeded, int Failed);