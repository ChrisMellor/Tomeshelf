namespace Tomeshelf.Web.Models.Shift;

public sealed record RedeemResultModel(int AccountId, string Email, string Service, bool Success, RedeemErrorCode? ErrorCode, string? Message);