namespace Tomeshelf.SHiFT.Api.Contracts;

/// <summary>
///     Represents a request to redeem a code for a specific service.
/// </summary>
/// <param name="Code">The code to be redeemed. Cannot be null or empty.</param>
/// <param name="Service">
///     The identifier of the service for which the code is being redeemed, or null to indicate the
///     default service.
/// </param>
public sealed record RedeemRequestDto(string Code, string? Service);