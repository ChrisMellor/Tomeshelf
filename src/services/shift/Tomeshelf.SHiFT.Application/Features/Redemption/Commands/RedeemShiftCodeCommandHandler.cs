using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.SHiFT.Application.Abstractions.External;
using Tomeshelf.SHiFT.Application.Features.Redemption.Redeem;

namespace Tomeshelf.SHiFT.Application.Features.Redemption.Commands;

public sealed class RedeemShiftCodeCommandHandler : ICommandHandler<RedeemShiftCodeCommand, IReadOnlyList<RedeemResult>>
{
    private readonly IGearboxClient _gearboxClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RedeemShiftCodeCommandHandler" /> class.
    /// </summary>
    /// <param name="gearboxClient">The gearbox client.</param>
    public RedeemShiftCodeCommandHandler(IGearboxClient gearboxClient)
    {
        _gearboxClient = gearboxClient;
    }

    /// <summary>
    ///     Handles.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public Task<IReadOnlyList<RedeemResult>> Handle(RedeemShiftCodeCommand command, CancellationToken cancellationToken)
    {
        return _gearboxClient.RedeemCodeAsync(command.Code, cancellationToken);
    }
}