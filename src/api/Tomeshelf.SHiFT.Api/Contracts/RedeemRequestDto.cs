namespace Tomeshelf.SHiFT.Api.Contracts;

/// <summary>
///     Represents a request to redeem a code for a specific service.
/// </summary>
/// <param name="Code">The code to be redeemed. Cannot be null or empty.</param>
/// <param name="Service">The name of the service for which the code is being redeemed.</param>
public sealed record RedeemRequestDto(string Code, string? Service);