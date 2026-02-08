using Bogus;
using FakeItEasy;
using Shouldly;
using Tomeshelf.SHiFT.Application.Abstractions.Common;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;
using Tomeshelf.SHiFT.Application.Abstractions.Security;
using Tomeshelf.SHiFT.Application.Features.Settings.Commands;
using Tomeshelf.SHiFT.Domain.Entities;

namespace Tomeshelf.SHiFT.Application.Tests.Features.Settings.Commands.CreateShiftSettingsCommandHandlerTests;

public class Handle
{
    [Fact]
    public async Task WhenEmailExists_Throws()
    {
        // Arrange
        var faker = new Faker();
        var repository = A.Fake<IShiftSettingsRepository>();
        var protector = A.Fake<ISecretProtector>();
        var clock = A.Fake<IClock>();
        var handler = new CreateShiftSettingsCommandHandler(repository, protector, clock);
        var email = faker.Internet.Email();

        A.CallTo(() => repository.EmailExistsAsync(email, null, A<CancellationToken>._))
         .Returns(Task.FromResult(true));

        // Act
        var command = new CreateShiftSettingsCommand(email, faker.Random.AlphaNumeric(8), "psn");

        // Assert
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        await Should.ThrowAsync<InvalidOperationException>(act);
    }

    [Fact]
    public async Task WithEmptyPassword_SetsNullEncryptedPassword()
    {
        // Arrange
        var faker = new Faker();
        var repository = A.Fake<IShiftSettingsRepository>();
        var protector = A.Fake<ISecretProtector>();
        var clock = A.Fake<IClock>();
        var handler = new CreateShiftSettingsCommandHandler(repository, protector, clock);
        var now = faker.Date.RecentOffset();
        var email = faker.Internet.Email();
        var service = "psn";

        A.CallTo(() => repository.EmailExistsAsync(email, null, A<CancellationToken>._))
         .Returns(Task.FromResult(false));
        A.CallTo(() => clock.UtcNow)
         .Returns(now);
        A.CallTo(() => repository.CreateAsync(A<SettingsEntity>._, A<CancellationToken>._))
         .Returns(Task.FromResult(7));

        var command = new CreateShiftSettingsCommand(email, string.Empty, service);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(7);
        A.CallTo(() => protector.Protect(A<string>._))
         .MustNotHaveHappened();
        A.CallTo(() => repository.CreateAsync(A<SettingsEntity>.That.Matches(entity => (entity.Email == email) && (entity.DefaultService == service) && (entity.EncryptedPassword == null) && (entity.UpdatedUtc == now)), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task WithNullPassword_SetsNullEncryptedPassword()
    {
        // Arrange
        var faker = new Faker();
        var repository = A.Fake<IShiftSettingsRepository>();
        var protector = A.Fake<ISecretProtector>();
        var clock = A.Fake<IClock>();
        var handler = new CreateShiftSettingsCommandHandler(repository, protector, clock);
        var now = faker.Date.RecentOffset();
        var email = faker.Internet.Email();
        var service = "psn";

        A.CallTo(() => repository.EmailExistsAsync(email, null, A<CancellationToken>._))
         .Returns(Task.FromResult(false));
        A.CallTo(() => clock.UtcNow)
         .Returns(now);
        A.CallTo(() => repository.CreateAsync(A<SettingsEntity>._, A<CancellationToken>._))
         .Returns(Task.FromResult(11));

        var command = new CreateShiftSettingsCommand(email, null, service);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(11);
        A.CallTo(() => protector.Protect(A<string>._))
         .MustNotHaveHappened();
        A.CallTo(() => repository.CreateAsync(A<SettingsEntity>.That.Matches(entity => (entity.Email == email) && (entity.DefaultService == service) && (entity.EncryptedPassword == null) && (entity.UpdatedUtc == now)), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task WithPassword_ProtectsAndStoresEncryptedPassword()
    {
        // Arrange
        var faker = new Faker();
        var repository = A.Fake<IShiftSettingsRepository>();
        var protector = A.Fake<ISecretProtector>();
        var clock = A.Fake<IClock>();
        var handler = new CreateShiftSettingsCommandHandler(repository, protector, clock);
        var now = faker.Date.RecentOffset();
        var email = faker.Internet.Email();
        var password = faker.Internet.Password();
        var service = "steam";
        var encrypted = faker.Random.AlphaNumeric(16);

        A.CallTo(() => repository.EmailExistsAsync(email, null, A<CancellationToken>._))
         .Returns(Task.FromResult(false));
        A.CallTo(() => clock.UtcNow)
         .Returns(now);
        A.CallTo(() => protector.Protect(password))
         .Returns(encrypted);
        A.CallTo(() => repository.CreateAsync(A<SettingsEntity>._, A<CancellationToken>._))
         .Returns(Task.FromResult(9));

        var command = new CreateShiftSettingsCommand(email, password, service);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(9);
        A.CallTo(() => protector.Protect(password))
         .MustHaveHappenedOnceExactly();
        A.CallTo(() => repository.CreateAsync(A<SettingsEntity>.That.Matches(entity => (entity.Email == email) && (entity.DefaultService == service) && (entity.EncryptedPassword == encrypted) && (entity.UpdatedUtc == now)), A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}