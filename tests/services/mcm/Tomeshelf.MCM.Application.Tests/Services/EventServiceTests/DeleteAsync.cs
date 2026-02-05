using Bogus;
using FakeItEasy;
using FluentAssertions;
using Tomeshelf.MCM.Application.Abstractions.Persistence;
using Tomeshelf.MCM.Application.Services;

namespace Tomeshelf.MCM.Application.Tests.Services.EventServiceTests;

public class DeleteAsync
{
    [Fact]
    public async Task CallsRepositoryAndReturnsResult()
    {
        // Arrange
        var faker = new Faker();
        var repository = A.Fake<IEventRepository>();
        var service = new EventService(repository);
        var id = faker.Random.AlphaNumeric(8);

        A.CallTo(() => repository.DeleteAsync(id, A<CancellationToken>._))
         .Returns(Task.FromResult(true));

        // Act
        var result = await service.DeleteAsync(id, CancellationToken.None);

        // Assert
        result.Should()
              .BeTrue();
        A.CallTo(() => repository.DeleteAsync(id, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}