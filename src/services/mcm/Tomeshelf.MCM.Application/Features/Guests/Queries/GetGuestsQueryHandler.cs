using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.MCM.Application.Contracts;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application.Features.Guests.Queries;

public sealed class GetGuestsQueryHandler : IQueryHandler<GetGuestsQuery, PagedResult<GuestDto>>
{
    private readonly IGuestsService _guestsService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetGuestsQueryHandler" /> class.
    /// </summary>
    /// <param name="guestsService">The guests service.</param>
    public GetGuestsQueryHandler(IGuestsService guestsService)
    {
        _guestsService = guestsService;
    }

    /// <summary>
    ///     Handles.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
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