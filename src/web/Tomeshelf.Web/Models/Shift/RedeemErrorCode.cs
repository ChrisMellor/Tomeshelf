namespace Tomeshelf.Web.Models.Shift;

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