using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application.Features.Events.Commands;

public sealed class DeleteEventCommandHandler : ICommandHandler<DeleteEventCommand, bool>
{
    private readonly IEventService _eventService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeleteEventCommandHandler" /> class.
    /// </summary>
    /// <param name="eventService">The event service.</param>
    public DeleteEventCommandHandler(IEventService eventService)
    {
        _eventService = eventService;
    }

    /// <summary>
    ///     Handles.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public Task<bool> Handle(DeleteEventCommand command, CancellationToken cancellationToken)
    {
        return _eventService.DeleteAsync(command.EventId, cancellationToken);
    }
}