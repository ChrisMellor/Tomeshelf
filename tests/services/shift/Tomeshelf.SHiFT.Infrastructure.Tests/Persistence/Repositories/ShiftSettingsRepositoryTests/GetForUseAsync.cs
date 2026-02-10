using System.Security.Cryptography;
using FakeItEasy;
using Shouldly;
using Tomeshelf.SHiFT.Application.Abstractions.Security;
using Tomeshelf.SHiFT.Domain.Entities;
using Tomeshelf.SHiFT.Infrastructure.Persistence.Repositories;
using Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Persistence.Repositories.ShiftSettingsRepositoryTests;

public class GetForUseAsync
{
    /// <summary>
    ///     Handles the cryptographic failure.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task HandlesCryptographicFailure()
    {
        // Arrange
        await using var context = await ShiftSettingsRepositoryTestHarness.CreateContextAsync();
        var protector = A.Fake<ISecretProtector>();
        A.CallTo(() => protector.Unprotect("enc-ok"))
         .Returns("password");
        A.CallTo(() => protector.Unprotect("enc-bad"))
         .Throws(new CryptographicException());

        context.ShiftSettings.AddRange(new SettingsEntity
        {
            Id = 1,
            Email = "user1@example.com",
            DefaultService = "psn",
            EncryptedPassword = "enc-ok"
        }, new SettingsEntity
        {
            Id = 2,
            Email = "user2@example.com",
            DefaultService = "steam",
            EncryptedPassword = "enc-bad"
        });
        await context.SaveChangesAsync();

        var repository = new ShiftSettingsRepository(context, protector);

        // Act
        var results = await repository.GetForUseAsync(CancellationToken.None);

        // Assert
        results.Count.ShouldBe(2);
        results.First(result => result.Id == 1)
               .Password
               .ShouldBe("password");
        results.First(result => result.Id == 2)
               .Password
               .ShouldBeEmpty();
    }

    /// <summary>
    ///     Unprotects twice when the password was protected twice.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task UnprotectsTwice_WhenPasswordWasProtectedTwice()
    {
        // Arrange
        await using var context = await ShiftSettingsRepositoryTestHarness.CreateContextAsync();
        var protector = A.Fake<ISecretProtector>();
        var inner = "CfDJ8" + new string('A', 80);

        A.CallTo(() => protector.Unprotect("enc-outer"))
         .Returns(inner);
        A.CallTo(() => protector.Unprotect(inner))
         .Returns("password");

        context.ShiftSettings.Add(new SettingsEntity
        {
            Id = 1,
            Email = "user@example.com",
            DefaultService = "psn",
            EncryptedPassword = "enc-outer"
        });
        await context.SaveChangesAsync();

        var repository = new ShiftSettingsRepository(context, protector);

        // Act
        var results = await repository.GetForUseAsync(CancellationToken.None);

        // Assert
        results.Count.ShouldBe(1);
        results[0].Password.ShouldBe("password");
        A.CallTo(() => protector.Unprotect("enc-outer"))
         .MustHaveHappenedOnceExactly();
        A.CallTo(() => protector.Unprotect(inner))
         .MustHaveHappenedOnceExactly();
    }

    /// <summary>
    ///     Returns empty when there are no rows.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ReturnsEmpty_WhenNoRows()
    {
        // Arrange
        await using var context = await ShiftSettingsRepositoryTestHarness.CreateContextAsync();
        var protector = A.Fake<ISecretProtector>();
        var repository = new ShiftSettingsRepository(context, protector);

        // Act
        var results = await repository.GetForUseAsync(CancellationToken.None);

        // Assert
        results.ShouldBeEmpty();
    }
}
