using FakeItEasy;
using Shouldly;
using Tomeshelf.SHiFT.Application.Abstractions.Security;
using Tomeshelf.SHiFT.Domain.Entities;
using Tomeshelf.SHiFT.Infrastructure.Persistence.Repositories;
using Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Persistence.Repositories.ShiftSettingsRepositoryTests;

public class EmailExistsAsync
{
    /// <summary>
    ///     Respects the excluding id.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task RespectsExcludingId()
    {
        // Arrange
        await using var context = await ShiftSettingsRepositoryTestHarness.CreateContextAsync();
        var protector = A.Fake<ISecretProtector>();
        var repository = new ShiftSettingsRepository(context, protector);

        context.ShiftSettings.Add(new SettingsEntity
        {
            Id = 5,
            Email = "user@example.com",
            DefaultService = "psn",
            EncryptedPassword = "enc"
        });
        await context.SaveChangesAsync();

        // Act
        var excluded = await repository.EmailExistsAsync("user@example.com", 5, CancellationToken.None);
        var included = await repository.EmailExistsAsync("user@example.com", 6, CancellationToken.None);

        // Assert
        excluded.ShouldBeFalse();
        included.ShouldBeTrue();
    }
}