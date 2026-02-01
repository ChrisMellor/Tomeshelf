using System.Collections.Generic;
using Tomeshelf.SHiFT.Application.Features.Redemption.Redeem;

namespace Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Models;

public sealed record ShiftKeySweepItem(string Code, IReadOnlyList<string> Sources, IReadOnlyList<RedeemResult> Results);
