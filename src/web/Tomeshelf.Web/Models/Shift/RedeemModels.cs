using System.Collections.Generic;

namespace Tomeshelf.Web.Models.Shift;

public sealed record RedeemRequestModel(string Code);

public sealed record RedeemResponseModel(RedeemSummaryModel Summary, IReadOnlyList<RedeemResultModel> Results);

public sealed record RedeemSummaryModel(int Total, int Succeeded, int Failed);

public sealed record RedeemResultModel(int AccountId, string Email, string Service, bool Success, RedeemErrorCode? ErrorCode, string? Message);

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