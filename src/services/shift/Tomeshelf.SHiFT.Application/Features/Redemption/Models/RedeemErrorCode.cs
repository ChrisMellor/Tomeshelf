namespace Tomeshelf.SHiFT.Application.Features.Redemption.Models;

/// <summary>
///     Specifies error codes that indicate the reason a redemption operation failed.
/// </summary>
/// <remarks>
///     Use this enumeration to determine the specific cause of a failed redemption attempt and to provide
///     appropriate feedback or error handling in your application. The values represent common failure scenarios such as
///     invalid credentials, missing CSRF tokens, unavailable redemption options, network issues, or misconfigured
///     accounts.
/// </remarks>
public enum RedeemErrorCode
{
    InvalidCredentials,
    CsrfMissing,
    NoRedemptionOptions,
    RedemptionFailed,
    NetworkError,
    AccountMisconfigured,
    Unknown
}