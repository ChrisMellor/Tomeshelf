using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Application.Abstractions.Messaging;
using Tomeshelf.MCM.Application.Contracts;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application.Features.Guests.Queries;

public sealed class GetGuestsQueryHandler : IQueryHandler<GetGuestsQuery, PagedResult<GuestDto>>
{
    private readonly IGuestsService _guestsService;

    public GetGuestsQueryHandler(IGuestsService guestsService)
    {
        _guestsService = guestsService;
    }

    public Task<PagedResult<GuestDto>> Handle(GetGuestsQuery query, CancellationToken cancellationToken)
    {
        var model = new EventConfigModel
        {
            Id = query.EventId,
            Name = query.EventName ?? string.Empty
        };

        return _guestsService.GetAsync(model, query.Page, query.PageSize, query.IncludeDeleted, cancellationToken);
    }
}
