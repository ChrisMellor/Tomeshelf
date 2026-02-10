using Bogus;
using FakeItEasy;
using Shouldly;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;
using Tomeshelf.SHiFT.Application.Features.Settings.Queries;
using Tomeshelf.SHiFT.Domain.Entities;

namespace Tomeshelf.SHiFT.Application.Tests.Features.Settings.Queries.GetShiftSettingsQueryHandlerTests;

public class Handle
{
    /// <summary>
    ///     Maps to dto when the entity exists.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
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
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(entity.Id);
        result.Email.ShouldBe(entity.Email);
        result.DefaultService.ShouldBe(entity.DefaultService);
        result.HasPassword.ShouldBeTrue();
        result.UpdatedUtc.ShouldBe(entity.UpdatedUtc);
    }

    /// <summary>
    ///     Returns null when missing.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
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
        result.ShouldBeNull();
    }

    /// <summary>
    ///     The sets has password false when the password is missing.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task WhenPasswordMissing_SetsHasPasswordFalse()
    {
        // Arrange
        var faker = new Faker();
        var repository = A.Fake<IShiftSettingsRepository>();
        var handler = new GetShiftSettingsQueryHandler(repository);
        var entity = new SettingsEntity
        {
            Id = 3,
            Email = faker.Internet.Email(),
            DefaultService = faker.Random.Word(),
            EncryptedPassword = "   ",
            UpdatedUtc = faker.Date.RecentOffset()
        };

        A.CallTo(() => repository.GetByIdAsync(3, A<CancellationToken>._))
         .Returns(Task.FromResult<SettingsEntity?>(entity));

        // Act
        var result = await handler.Handle(new GetShiftSettingsQuery(3), CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result!.HasPassword.ShouldBeFalse();
    }
}