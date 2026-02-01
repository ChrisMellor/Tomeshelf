using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.MCM.Application.Contracts;

namespace Tomeshelf.MCM.Application.Features.Guests.Queries;

public sealed record GetGuestsQuery(string EventId, int Page, int PageSize, string? EventName, bool IncludeDeleted) : IQuery<PagedResult<GuestDto>>;
