using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application.Features.Events.Queries;

public sealed class GetEventsQueryHandler : IQueryHandler<GetEventsQuery, IReadOnlyList<EventConfigModel>>
{
    private readonly IEventService _eventService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetEventsQueryHandler" /> class.
    /// </summary>
    /// <param name="eventService">The event service.</param>
    public GetEventsQueryHandler(IEventService eventService)
    {
        _eventService = eventService;
    }

    /// <summary>
    ///     Handles.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public Task<IReadOnlyList<EventConfigModel>> Handle(GetEventsQuery query, CancellationToken cancellationToken)
    {
        return _eventService.GetAllAsync(cancellationToken);
    }
}