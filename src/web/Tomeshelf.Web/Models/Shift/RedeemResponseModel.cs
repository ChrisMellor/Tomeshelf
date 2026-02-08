using System.Collections.Generic;

namespace Tomeshelf.Web.Models.Shift;

public sealed record RedeemResponseModel(RedeemSummaryModel Summary, IReadOnlyList<RedeemResultModel> Results);