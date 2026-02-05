using Bogus;
using FakeItEasy;
using FluentAssertions;
using Tomeshelf.MCM.Application.Abstractions.Persistence;
using Tomeshelf.MCM.Application.Models;
using Tomeshelf.MCM.Application.Services;
using Tomeshelf.MCM.Domain.Mcm;

namespace Tomeshelf.MCM.Application.Tests.Services.EventServiceTests;

public class GetAllAsync
{
    [Fact]
    public async Task MapsEntitiesToModels()
    {
        // Arrange
        var faker = new Faker();
        var repository = A.Fake<IEventRepository>();
        var service = new EventService(repository);
        var entities = new List<EventEntity>
        {
            new EventEntity
            {
                Id = faker.Random.AlphaNumeric(8),
                Name = faker.Company.CompanyName()
            },
            new EventEntity
            {
                Id = faker.Random.AlphaNumeric(8),
                Name = faker.Company.CompanyName()
            }
        };

        var expected = entities.Select(entity => new EventConfigModel
                                {
                                    Id = entity.Id,
                                    Name = entity.Name
                                })
                               .ToList();

        A.CallTo(() => repository.GetAllAsync(A<CancellationToken>._))
         .Returns(Task.FromResult<IReadOnlyList<EventEntity>>(entities));

        // Act
        var result = await service.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should()
              .BeEquivalentTo(expected, options => options.WithStrictOrdering());
    }
}