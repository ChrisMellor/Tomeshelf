using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.SHiFT.Application.Abstractions.External;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.SHiFT.Application.Features.Redemption.Redeem;

namespace Tomeshelf.SHiFT.Application.Features.Redemption.Commands;

public sealed class RedeemShiftCodeCommandHandler : ICommandHandler<RedeemShiftCodeCommand, IReadOnlyList<RedeemResult>>
{
    private readonly IGearboxClient _gearboxClient;

    public RedeemShiftCodeCommandHandler(IGearboxClient gearboxClient)
    {
        _gearboxClient = gearboxClient;
    }

    public Task<IReadOnlyList<RedeemResult>> Handle(RedeemShiftCodeCommand command, CancellationToken cancellationToken)
    {
        return _gearboxClient.RedeemCodeAsync(command.Code, cancellationToken);
    }
}
