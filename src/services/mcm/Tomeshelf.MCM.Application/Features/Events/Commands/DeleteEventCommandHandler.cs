using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application.Features.Events.Commands;

public sealed class DeleteEventCommandHandler : ICommandHandler<DeleteEventCommand, bool>
{
    private readonly IEventService _eventService;

    public DeleteEventCommandHandler(IEventService eventService)
    {
        _eventService = eventService;
    }

    public Task<bool> Handle(DeleteEventCommand command, CancellationToken cancellationToken)
    {
        return _eventService.DeleteAsync(command.EventId, cancellationToken);
    }
}