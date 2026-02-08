using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tomeshelf.SHiFT.Application.Abstractions.Security;
using Tomeshelf.SHiFT.Domain.Entities;
using Tomeshelf.SHiFT.Infrastructure.Persistence.Repositories;
using Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Persistence.Repositories.ShiftSettingsRepositoryTests;

public class CreateAsync
{
    [Fact]
    public async Task PersistsTrimmedValues_AndProtectsPassword()
    {
        // Arrange
        await using var context = await ShiftSettingsRepositoryTestHarness.CreateContextAsync();
        var protector = A.Fake<ISecretProtector>();
        A.CallTo(() => protector.Protect("secret"))
         .Returns("encrypted");

        var repository = new ShiftSettingsRepository(context, protector);
        var request = new SettingsEntity
        {
            Email = "  user@example.com ",
            DefaultService = " steam ",
            EncryptedPassword = " secret "
        };

        var before = DateTimeOffset.UtcNow;
        var id = await repository.CreateAsync(request, CancellationToken.None);
        var after = DateTimeOffset.UtcNow;

        // Act
        var stored = await context.ShiftSettings.SingleAsync(entity => entity.Id == id);
        // Assert
        stored.Email.ShouldBe("user@example.com");
        stored.DefaultService.ShouldBe("steam");
        stored.EncryptedPassword.ShouldBe("encrypted");
        stored.UpdatedUtc.ShouldBeInRange(before, after);
    }

    [Fact]
    public async Task Throws_WhenEmailAlreadyExists()
    {
        // Arrange
        await using var context = await ShiftSettingsRepositoryTestHarness.CreateContextAsync();
        var protector = A.Fake<ISecretProtector>();
        var repository = new ShiftSettingsRepository(context, protector);

        context.ShiftSettings.Add(new SettingsEntity
        {
            Email = "user@example.com",
            DefaultService = "psn",
            EncryptedPassword = "enc"
        });
        await context.SaveChangesAsync();

        var request = new SettingsEntity
        {
            Email = "user@example.com",
            DefaultService = "psn",
            EncryptedPassword = "secret"
        };

        var action = () => repository.CreateAsync(request, CancellationToken.None);

        // Act
        var exception = await Should.ThrowAsync<InvalidOperationException>(action);
        // Assert
        exception.Message.ShouldBe("SHiFT email already exists");
    }

    [Theory]
    [InlineData("", "psn", "secret", "email", "Missing email")]
    [InlineData("user@example.com", "", "secret", "service", "Missing service")]
    [InlineData("user@example.com", "psn", " ", "password", "Missing password")]
    public async Task ThrowsWhenRequiredValuesMissing(string email, string service, string password, string paramName, string expectedMessage)
    {
        // Arrange
        await using var context = await ShiftSettingsRepositoryTestHarness.CreateContextAsync();
        var protector = A.Fake<ISecretProtector>();
        var repository = new ShiftSettingsRepository(context, protector);

        var request = new SettingsEntity
        {
            Email = email,
            DefaultService = service,
            EncryptedPassword = password
        };

        var action = () => repository.CreateAsync(request, CancellationToken.None);

        // Act
        var exception = await Should.ThrowAsync<ArgumentNullException>(action);
        // Assert
        exception.ParamName.ShouldBe(paramName);
        exception.Message.ShouldContain(expectedMessage);
    }
}