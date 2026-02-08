using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.MCM.Application.Abstractions.Clients;
using Tomeshelf.MCM.Application.Abstractions.Mappers;
using Tomeshelf.MCM.Application.Abstractions.Persistence;
using Tomeshelf.MCM.Application.Contracts;
using Tomeshelf.MCM.Application.Features.Events.Commands;
using Tomeshelf.MCM.Application.Features.Events.Queries;
using Tomeshelf.MCM.Application.Features.Guests.Commands;
using Tomeshelf.MCM.Application.Features.Guests.Queries;
using Tomeshelf.MCM.Application.Mappers;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application.Tests.DependencyInjectionTests;

public class AddApplicationServices
{
    [Fact]
    public void RegistersExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(A.Fake<IMcmGuestsClient>());
        services.AddSingleton(A.Fake<IGuestsRepository>());
        services.AddSingleton(A.Fake<IEventRepository>());

        // Act
        services.AddApplicationServices();

        // Assert
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IGuestMapper>()
                .ShouldBeOfType<GuestMapper>();
        provider.GetRequiredService<IGuestsService>()
                .ShouldBeOfType<GuestsService>();
        provider.GetRequiredService<IEventService>()
                .ShouldBeOfType<EventService>();
        provider.GetRequiredService<IQueryHandler<GetGuestsQuery, PagedResult<GuestDto>>>()
                .ShouldBeOfType<GetGuestsQueryHandler>();
        provider.GetRequiredService<ICommandHandler<SyncGuestsCommand, GuestSyncResultDto?>>()
                .ShouldBeOfType<SyncGuestsCommandHandler>();
        provider.GetRequiredService<IQueryHandler<GetEventsQuery, IReadOnlyList<EventConfigModel>>>()
                .ShouldBeOfType<GetEventsQueryHandler>();
        provider.GetRequiredService<ICommandHandler<UpsertEventCommand, bool>>()
                .ShouldBeOfType<UpsertEventCommandHandler>();
        provider.GetRequiredService<ICommandHandler<DeleteEventCommand, bool>>()
                .ShouldBeOfType<DeleteEventCommandHandler>();
    }
}