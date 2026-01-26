using System.Collections.Generic;
using Tomeshelf.SHiFT.Application.Abstractions.Messaging;
using Tomeshelf.SHiFT.Application.Features.Redemption.Redeem;

namespace Tomeshelf.SHiFT.Application.Features.Redemption.Commands;

public sealed record RedeemShiftCodeCommand(string Code) : ICommand<IReadOnlyList<RedeemResult>>;
