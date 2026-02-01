using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application.Features.Events.Commands;

public sealed class UpsertEventCommandHandler : ICommandHandler<UpsertEventCommand, bool>
{
    private readonly IEventService _eventService;

    public UpsertEventCommandHandler(IEventService eventService)
    {
        _eventService = eventService;
    }

    public async Task<bool> Handle(UpsertEventCommand command, CancellationToken cancellationToken)
    {
        await _eventService.UpsertAsync(command.Model, cancellationToken);
        return true;
    }
}
