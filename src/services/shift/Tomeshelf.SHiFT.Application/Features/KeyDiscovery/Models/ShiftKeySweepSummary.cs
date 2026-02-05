namespace Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Models;

public sealed record ShiftKeySweepSummary(int TotalKeys, int TotalRedemptionAttempts, int TotalSucceeded, int TotalFailed);