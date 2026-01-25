namespace Tomeshelf.SHiFT.Api.Contracts;

/// <summary>
///     Represents a request to redeem a code for a specific service.
/// </summary>
/// <param name="Code">The code to be redeemed. Cannot be null or empty.</param>
public sealed record RedeemRequestDto(string Code);