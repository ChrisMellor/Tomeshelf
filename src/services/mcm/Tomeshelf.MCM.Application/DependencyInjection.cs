using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Tomeshelf.MCM.Application.Abstractions.Mappers;
using Tomeshelf.MCM.Application.Abstractions.Messaging;
using Tomeshelf.MCM.Application.Contracts;
using Tomeshelf.MCM.Application.Features.Events.Commands;
using Tomeshelf.MCM.Application.Features.Events.Queries;
using Tomeshelf.MCM.Application.Features.Guests.Commands;
using Tomeshelf.MCM.Application.Features.Guests.Queries;
using Tomeshelf.MCM.Application.Mappers;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IGuestMapper, GuestMapper>();
        services.AddScoped<IGuestsService, GuestsService>();
        services.AddScoped<IEventService, EventService>();

        services.AddScoped<IQueryHandler<GetGuestsQuery, PagedResult<GuestDto>>, GetGuestsQueryHandler>();
        services.AddScoped<ICommandHandler<SyncGuestsCommand, GuestSyncResultDto?>, SyncGuestsCommandHandler>();
        services.AddScoped<IQueryHandler<GetEventsQuery, IReadOnlyList<EventConfigModel>>, GetEventsQueryHandler>();
        services.AddScoped<ICommandHandler<UpsertEventCommand, bool>, UpsertEventCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteEventCommand, bool>, DeleteEventCommandHandler>();

        return services;
    }
}
