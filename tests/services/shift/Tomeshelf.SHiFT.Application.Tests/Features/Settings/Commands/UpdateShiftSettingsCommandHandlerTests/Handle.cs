using Bogus;
using FakeItEasy;
using Tomeshelf.SHiFT.Application.Abstractions.Common;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;
using Tomeshelf.SHiFT.Application.Abstractions.Security;
using Tomeshelf.SHiFT.Application.Features.Settings.Commands;
using Tomeshelf.SHiFT.Domain.Entities;

namespace Tomeshelf.SHiFT.Application.Tests.Features.Settings.Commands.UpdateShiftSettingsCommandHandlerTests;

public class Handle
{
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
        await Should.ThrowAsync<InvalidOperationException>(act);
    }

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
        result.ShouldBeFalse();
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
        var entity = new SettingsEntity
        {
            Id = 1,
            Email = faker.Internet.Email()
        };
        var now = faker.Date.RecentOffset();
        var newEmail = faker.Internet.Email();
        var password = faker.Internet.Password();
        var encrypted = faker.Random.AlphaNumeric(16);

        A.CallTo(() => repository.GetByIdAsync(1, A<CancellationToken>._))
         .Returns(Task.FromResult<SettingsEntity?>(entity));
        A.CallTo(() => repository.EmailExistsAsync(newEmail, 1, A<CancellationToken>._))
         .Returns(Task.FromResult(false));
        A.CallTo(() => protector.Protect(password))
         .Returns(encrypted);
        A.CallTo(() => clock.UtcNow)
         .Returns(now);

        var command = new UpdateShiftSettingsCommand(1, newEmail, password, "xbox");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
        entity.Email.ShouldBe(newEmail);
        entity.DefaultService.ShouldBe("xbox");
        entity.EncryptedPassword.ShouldBe(encrypted);
        entity.UpdatedUtc.ShouldBe(now);
        A.CallTo(() => repository.UpdateAsync(1, entity, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task PasswordNull_DoesNotChangeEncryptedPassword()
    {
        // Arrange
        var faker = new Faker();
        var repository = A.Fake<IShiftSettingsRepository>();
        var protector = A.Fake<ISecretProtector>();
        var clock = A.Fake<IClock>();
        var handler = new UpdateShiftSettingsCommandHandler(repository, protector, clock);
        var entity = new SettingsEntity
        {
            Id = 4,
            Email = faker.Internet.Email(),
            DefaultService = "psn",
            EncryptedPassword = "existing"
        };

        var now = faker.Date.RecentOffset();
        A.CallTo(() => repository.GetByIdAsync(4, A<CancellationToken>._))
         .Returns(Task.FromResult<SettingsEntity?>(entity));
        A.CallTo(() => repository.EmailExistsAsync(A<string>._, 4, A<CancellationToken>._))
         .Returns(Task.FromResult(false));
        A.CallTo(() => clock.UtcNow)
         .Returns(now);

        var command = new UpdateShiftSettingsCommand(4, faker.Internet.Email(), null, "steam");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
        entity.EncryptedPassword.ShouldBe("existing");
        A.CallTo(() => protector.Protect(A<string>._))
         .MustNotHaveHappened();
    }

    [Fact]
    public async Task EmptyPassword_SetsEncryptedPasswordToNull()
    {
        // Arrange
        var faker = new Faker();
        var repository = A.Fake<IShiftSettingsRepository>();
        var protector = A.Fake<ISecretProtector>();
        var clock = A.Fake<IClock>();
        var handler = new UpdateShiftSettingsCommandHandler(repository, protector, clock);
        var entity = new SettingsEntity
        {
            Id = 9,
            Email = faker.Internet.Email(),
            DefaultService = "psn",
            EncryptedPassword = "existing"
        };

        var now = faker.Date.RecentOffset();
        A.CallTo(() => repository.GetByIdAsync(9, A<CancellationToken>._))
         .Returns(Task.FromResult<SettingsEntity?>(entity));
        A.CallTo(() => repository.EmailExistsAsync(A<string>._, 9, A<CancellationToken>._))
         .Returns(Task.FromResult(false));
        A.CallTo(() => clock.UtcNow)
         .Returns(now);

        var command = new UpdateShiftSettingsCommand(9, faker.Internet.Email(), string.Empty, "steam");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
        entity.EncryptedPassword.ShouldBeNull();
        A.CallTo(() => protector.Protect(A<string>._))
         .MustNotHaveHappened();
    }
}
