using Bogus;
using FakeItEasy;
using FluentAssertions;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;
using Tomeshelf.SHiFT.Application.Features.Settings.Queries;
using Tomeshelf.SHiFT.Domain.Entities;

namespace Tomeshelf.SHiFT.Application.Tests.Features.Settings.Queries.GetShiftSettingsQueryHandlerTests;

public class Handle
{
    [Fact]
    public async Task WhenEntityExists_MapsToDto()
    {
        // Arrange
        var faker = new Faker();
        var repository = A.Fake<IShiftSettingsRepository>();
        var handler = new GetShiftSettingsQueryHandler(repository);
        var entity = new SettingsEntity
        {
            Id = 2,
            Email = faker.Internet.Email(),
            DefaultService = faker.Random.Word(),
            EncryptedPassword = faker.Random.AlphaNumeric(12),
            UpdatedUtc = faker.Date.RecentOffset()
        };

        A.CallTo(() => repository.GetByIdAsync(2, A<CancellationToken>._))
         .Returns(Task.FromResult<SettingsEntity?>(entity));

        // Act
        var result = await handler.Handle(new GetShiftSettingsQuery(2), CancellationToken.None);

        // Assert
        result.Should()
              .NotBeNull();
        result!.Id
               .Should()
               .Be(entity.Id);
        result.Email
              .Should()
              .Be(entity.Email);
        result.DefaultService
              .Should()
              .Be(entity.DefaultService);
        result.HasPassword
              .Should()
              .BeTrue();
        result.UpdatedUtc
              .Should()
              .Be(entity.UpdatedUtc);
    }

    [Fact]
    public async Task WhenMissing_ReturnsNull()
    {
        // Arrange
        var repository = A.Fake<IShiftSettingsRepository>();
        var handler = new GetShiftSettingsQueryHandler(repository);

        A.CallTo(() => repository.GetByIdAsync(1, A<CancellationToken>._))
         .Returns(Task.FromResult<SettingsEntity?>(null));

        // Act
        var result = await handler.Handle(new GetShiftSettingsQuery(1), CancellationToken.None);

        // Assert
        result.Should()
              .BeNull();
    }
}