namespace Tomeshelf.SHiFT.Api.Contracts;

/// <summary>
///     Represents aggregate counts for a SHiFT key sweep.
/// </summary>
/// <param name="TotalKeys">The total number of distinct SHiFT keys found.</param>
/// <param name="TotalRedemptionAttempts">Total redemption attempts executed across all keys.</param>
/// <param name="TotalSucceeded">The number of successful redemption attempts.</param>
/// <param name="TotalFailed">The number of failed redemption attempts.</param>
public sealed record ShiftKeySweepSummaryDto(int TotalKeys, int TotalRedemptionAttempts, int TotalSucceeded, int TotalFailed);
