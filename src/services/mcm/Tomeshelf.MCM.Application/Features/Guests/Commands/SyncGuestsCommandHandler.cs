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

    public SyncGuestsCommandHandler(IGuestsService guestsService)
    {
        _guestsService = guestsService;
    }

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