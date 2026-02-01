using Bogus;
using FakeItEasy;
using Tomeshelf.MCM.Application.Abstractions.Persistence;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application.Tests.Services.EventServiceTests;

public class UpsertAsync
{
    [Fact]
    public async Task CallsRepository()
    {
        // Arrange
        var faker = new Faker();
        var repository = A.Fake<IEventRepository>();
        var service = new EventService(repository);
        var model = new EventConfigModel { Id = faker.Random.AlphaNumeric(8), Name = faker.Company.CompanyName() };

        // Act
        await service.UpsertAsync(model, CancellationToken.None);

        // Assert
        A.CallTo(() => repository.UpsertAsync(model, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
