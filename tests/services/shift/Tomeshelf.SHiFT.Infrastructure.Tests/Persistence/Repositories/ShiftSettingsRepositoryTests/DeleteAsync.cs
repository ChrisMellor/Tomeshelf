using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tomeshelf.SHiFT.Application.Abstractions.Security;
using Tomeshelf.SHiFT.Domain.Entities;
using Tomeshelf.SHiFT.Infrastructure.Persistence.Repositories;
using Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Persistence.Repositories.ShiftSettingsRepositoryTests;

public class DeleteAsync
{
    [Fact]
    public async Task RemovesMatchingRow()
    {
        // Arrange
        await using var context = await ShiftSettingsRepositoryTestHarness.CreateSqliteContextAsync();
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

        // Act
        await repository.DeleteAsync(1, CancellationToken.None);

        // Assert
        var remaining = await context.ShiftSettings.ToListAsync();
        remaining.ShouldHaveSingleItem();
        remaining[0]
           .Id
           .ShouldBe(2);
    }
}