using FakeItEasy;
using Shouldly;
using Tomeshelf.SHiFT.Application.Abstractions.Security;
using Tomeshelf.SHiFT.Domain.Entities;
using Tomeshelf.SHiFT.Infrastructure.Persistence.Repositories;
using Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Persistence.Repositories.ShiftSettingsRepositoryTests;

public class GetByIdAsync
{
    [Fact]
    public async Task ReturnsEntity_WhenFound()
    {
        await using var context = await ShiftSettingsRepositoryTestHarness.CreateContextAsync();
        var protector = A.Fake<ISecretProtector>();
        context.ShiftSettings.Add(new SettingsEntity
        {
            Id = 3,
            Email = "user@example.com",
            DefaultService = "psn",
            EncryptedPassword = "enc"
        });
        await context.SaveChangesAsync();
        var repository = new ShiftSettingsRepository(context, protector);

        var result = await repository.GetByIdAsync(3, CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(3);
        result.Email.ShouldBe("user@example.com");
        result.DefaultService.ShouldBe("psn");
        result.EncryptedPassword.ShouldBe("enc");
    }

    [Fact]
    public async Task ReturnsNull_WhenMissing()
    {
        await using var context = await ShiftSettingsRepositoryTestHarness.CreateContextAsync();
        var protector = A.Fake<ISecretProtector>();
        var repository = new ShiftSettingsRepository(context, protector);

        var result = await repository.GetByIdAsync(42, CancellationToken.None);

        result.ShouldBeNull();
    }
}