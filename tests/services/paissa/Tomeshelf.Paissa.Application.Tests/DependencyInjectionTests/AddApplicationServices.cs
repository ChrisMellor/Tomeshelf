using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Paissa.Application.Abstractions.Common;
using Tomeshelf.Paissa.Application.Abstractions.External;
using Tomeshelf.Paissa.Application.Features.Housing.Dtos;
using Tomeshelf.Paissa.Application.Features.Housing.Queries;

namespace Tomeshelf.Paissa.Application.Tests.DependencyInjectionTests;

public class AddApplicationServices
{
    [Fact]
    public void RegistersHandlers()
    {
        var services = new ServiceCollection();
        services.AddSingleton(A.Fake<IPaissaClient>());
        services.AddSingleton(A.Fake<IPaissaWorldSettings>());
        services.AddSingleton(A.Fake<IClock>());

        services.AddApplicationServices();

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IQueryHandler<GetAcceptingEntriesQuery, PaissaWorldSummaryDto>>()
                .ShouldBeOfType<GetAcceptingEntriesQueryHandler>();
    }
}