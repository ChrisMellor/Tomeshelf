using Bogus;
using FakeItEasy;
using FluentAssertions;
using Tomeshelf.SHiFT.Application.Abstractions.Common;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;
using Tomeshelf.SHiFT.Application.Abstractions.Security;
using Tomeshelf.SHiFT.Application.Features.Settings.Commands;
using Tomeshelf.SHiFT.Domain.Entities;

namespace Tomeshelf.SHiFT.Application.Tests.Features.Settings.Commands.UpdateShiftSettingsCommandHandlerTests;

public class Handle
{
    [Fact]
    public async Task EntityNotFound_ReturnsFalse()
    {
        // Arrange
        var faker = new Faker();
        var repository = A.Fake<IShiftSettingsRepository>();
        var protector = A.Fake<ISecretProtector>();
        var clock = A.Fake<IClock>();
        var handler = new UpdateShiftSettingsCommandHandler(repository, protector, clock);

        A.CallTo(() => repository.GetByIdAsync(A<int>._, A<CancellationToken>._))
            .Returns(Task.FromResult<SettingsEntity?>(null));

        var command = new UpdateShiftSettingsCommand(1, faker.Internet.Email(), faker.Internet.Password(), "psn");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EmailAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var faker = new Faker();
        var repository = A.Fake<IShiftSettingsRepository>();
        var protector = A.Fake<ISecretProtector>();
        var clock = A.Fake<IClock>();
        var handler = new UpdateShiftSettingsCommandHandler(repository, protector, clock);
        var entity = new SettingsEntity { Id = 1 };
        var email = faker.Internet.Email();

        A.CallTo(() => repository.GetByIdAsync(1, A<CancellationToken>._))
            .Returns(Task.FromResult<SettingsEntity?>(entity));
        A.CallTo(() => repository.EmailExistsAsync(email, 1, A<CancellationToken>._))
            .Returns(Task.FromResult(true));

        var command = new UpdateShiftSettingsCommand(1, email, faker.Internet.Password(), "psn");

        // Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ValidCommandWithPassword_UpdatesEntityAndReturnsTrue()
    {
        // Arrange
        var faker = new Faker();
        var repository = A.Fake<IShiftSettingsRepository>();
        var protector = A.Fake<ISecretProtector>();
        var clock = A.Fake<IClock>();
        var handler = new UpdateShiftSettingsCommandHandler(repository, protector, clock);
        var entity = new SettingsEntity { Id = 1, Email = faker.Internet.Email() };
        var now = faker.Date.RecentOffset();
        var newEmail = faker.Internet.Email();
        var password = faker.Internet.Password();
        var encrypted = faker.Random.AlphaNumeric(16);

        A.CallTo(() => repository.GetByIdAsync(1, A<CancellationToken>._))
            .Returns(Task.FromResult<SettingsEntity?>(entity));
        A.CallTo(() => repository.EmailExistsAsync(newEmail, 1, A<CancellationToken>._))
            .Returns(Task.FromResult(false));
        A.CallTo(() => protector.Protect(password)).Returns(encrypted);
        A.CallTo(() => clock.UtcNow).Returns(now);

        var command = new UpdateShiftSettingsCommand(1, newEmail, password, "xbox");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        entity.Email.Should().Be(newEmail);
        entity.DefaultService.Should().Be("xbox");
        entity.EncryptedPassword.Should().Be(encrypted);
        entity.UpdatedUtc.Should().Be(now);
        A.CallTo(() => repository.UpdateAsync(1, entity, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
