namespace Tomeshelf.Application.Shared.Contracts.SHiFT;

/// <summary>
///     Represents the result of a service redemption attempt, including status, account information, and error details if
///     applicable.
/// </summary>
/// <param name="AccountId">The unique identifier of the account for which the redemption was attempted.</param>
/// <param name="Email">The email address associated with the account involved in the redemption.</param>
/// <param name="Service">The name of the service for which the redemption was performed.</param>
/// <param name="Success">
///     A value indicating whether the redemption was successful. Set to <see langword="true" /> if the operation
///     succeeded; otherwise, <see langword="false" />.
/// </param>
/// <param name="ErrorCode">
///     The error code describing the reason for a failed redemption, or <see langword="null" /> if the operation was
///     successful.
/// </param>
/// <param name="Message">
///     An optional message providing additional information about the redemption result. May be
///     <see langword="null" />.
/// </param>
public sealed record RedeemResult(int AccountId, string Email, string Service, bool Success, RedeemErrorCode? ErrorCode, string? Message);