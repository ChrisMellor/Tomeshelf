using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.MCM.Application.Contracts;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application.Features.Guests.Commands;

public sealed class SyncGuestsCommandHandler : ICommandHandler<SyncGuestsCommand, GuestSyncResultDto?>
{
    private readonly IGuestsService _guestsService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SyncGuestsCommandHandler" /> class.
    /// </summary>
    /// <param name="guestsService">The guests service.</param>
    public SyncGuestsCommandHandler(IGuestsService guestsService)
    {
        _guestsService = guestsService;
    }

    /// <summary>
    ///     Handles.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public Task<GuestSyncResultDto?> Handle(SyncGuestsCommand command, CancellationToken cancellationToken)
    {
        var model = new EventConfigModel
        {
            Id = command.EventId,
            Name = string.Empty
        };

        return _guestsService.SyncAsync(model, cancellationToken);
    }
}