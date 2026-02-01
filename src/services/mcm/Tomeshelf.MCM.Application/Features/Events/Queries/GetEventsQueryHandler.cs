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

    public GetEventsQueryHandler(IEventService eventService)
    {
        _eventService = eventService;
    }

    public Task<IReadOnlyList<EventConfigModel>> Handle(GetEventsQuery query, CancellationToken cancellationToken)
    {
        return _eventService.GetAllAsync(cancellationToken);
    }
}
