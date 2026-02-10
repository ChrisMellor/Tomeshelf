using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application.Features.Events.Commands;

public sealed class UpsertEventCommandHandler : ICommandHandler<UpsertEventCommand, bool>
{
    private readonly IEventService _eventService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UpsertEventCommandHandler" /> class.
    /// </summary>
    /// <param name="eventService">The event service.</param>
    public UpsertEventCommandHandler(IEventService eventService)
    {
        _eventService = eventService;
    }

    /// <summary>
    ///     Handles.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public async Task<bool> Handle(UpsertEventCommand command, CancellationToken cancellationToken)
    {
        await _eventService.UpsertAsync(command.Model, cancellationToken);

        return true;
    }
}