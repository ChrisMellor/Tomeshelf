using FakeItEasy;
using Tomeshelf.MCM.Application.Abstractions.Clients;
using Tomeshelf.MCM.Application.Abstractions.Mappers;
using Tomeshelf.MCM.Application.Abstractions.Persistence;
using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application.Tests.TestUtilities;

public static class GuestsServiceTestHarness
{
    /// <summary>
    ///     Creates the service.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public static (GuestsService Service, IMcmGuestsClient Client, IGuestMapper Mapper, IGuestsRepository Repository) CreateService()
    {
        var client = A.Fake<IMcmGuestsClient>();
        var mapper = A.Fake<IGuestMapper>();
        var repository = A.Fake<IGuestsRepository>();
        var service = new GuestsService(client, mapper, repository);

        return (service, client, mapper, repository);
    }
}
