using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tomeshelf.SHiFT.Application.Abstractions.Security;
using Tomeshelf.SHiFT.Domain.Entities;
using Tomeshelf.SHiFT.Infrastructure.Persistence.Repositories;
using Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Persistence.Repositories.ShiftSettingsRepositoryTests;

public class UpdateAsync
{
    /// <summary>
    ///     Returns a result when the row is missing.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task Returns_WhenRowMissing()
    {
        // Arrange
        await using var context = await ShiftSettingsRepositoryTestHarness.CreateContextAsync();
        var protector = A.Fake<ISecretProtector>();
        var repository = new ShiftSettingsRepository(context, protector);
        var update = new SettingsEntity
        {
            Email = "user@example.com",
            DefaultService = "psn",
            EncryptedPassword = "enc"
        };

        // Act
        await repository.UpdateAsync(999, update, CancellationToken.None);

        // Assert
        var count = await context.ShiftSettings.CountAsync();
        count.ShouldBe(0);
    }

    /// <summary>
    ///     Throws when the duplicate email exists.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task Throws_WhenDuplicateEmailExists()
    {
        // Arrange
        await using var context = await ShiftSettingsRepositoryTestHarness.CreateContextAsync();
        var protector = A.Fake<ISecretProtector>();

        context.ShiftSettings.AddRange(new SettingsEntity
        {
            Id = 1,
            Email = "user1@example.com",
            DefaultService = "psn",
            EncryptedPassword = "enc"
        }, new SettingsEntity
        {
            Id = 2,
            Email = "user2@example.com",
            DefaultService = "steam",
            EncryptedPassword = "enc"
        });
        await context.SaveChangesAsync();

        var repository = new ShiftSettingsRepository(context, protector);
        var update = new SettingsEntity
        {
            Email = "user2@example.com",
            DefaultService = "psn",
            EncryptedPassword = "enc"
        };

        // Act
        var action = () => repository.UpdateAsync(1, update, CancellationToken.None);

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(action);
    }

    /// <summary>
    ///     Throws when the required values are missing.
    /// </summary>
    /// <param name="email">The email.</param>
    /// <param name="service">The service.</param>
    /// <param name="expectedParam">The expected param.</param>
    /// <param name="expectedMessage">The expected message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Theory]
    [InlineData("", "psn", "Email", "Missing email")]
    [InlineData("user@example.com", " ", "DefaultService", "Missing service")]
    public async Task Throws_WhenRequiredValuesMissing(string email, string service, string expectedParam, string expectedMessage)
    {
        // Arrange
        await using var context = await ShiftSettingsRepositoryTestHarness.CreateContextAsync();
        var protector = A.Fake<ISecretProtector>();
        var repository = new ShiftSettingsRepository(context, protector);
        var update = new SettingsEntity
        {
            Email = email,
            DefaultService = service,
            EncryptedPassword = "enc"
        };

        // Act
        var action = () => repository.UpdateAsync(1, update, CancellationToken.None);

        // Assert
        var exception = await Should.ThrowAsync<ArgumentNullException>(action);
        exception.ParamName.ShouldBe(expectedParam);
        exception.Message.ShouldContain(expectedMessage);
    }

    /// <summary>
    ///     Updates row when the value is valid.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UpdatesRow_WhenValid()
    {
        // Arrange
        await using var context = await ShiftSettingsRepositoryTestHarness.CreateContextAsync();
        var protector = A.Fake<ISecretProtector>();

        context.ShiftSettings.Add(new SettingsEntity
        {
            Id = 1,
            Email = "user1@example.com",
            DefaultService = "psn",
            EncryptedPassword = "enc"
        });
        await context.SaveChangesAsync();

        var repository = new ShiftSettingsRepository(context, protector);
        var update = new SettingsEntity
        {
            Email = "updated@example.com",
            DefaultService = "steam",
            EncryptedPassword = "new-enc"
        };

        // Act
        await repository.UpdateAsync(1, update, CancellationToken.None);

        // Assert
        var stored = await context.ShiftSettings.SingleAsync(entity => entity.Id == 1);
        stored.Email.ShouldBe("updated@example.com");
        stored.DefaultService.ShouldBe("steam");
        stored.EncryptedPassword.ShouldBe("new-enc");
    }
}