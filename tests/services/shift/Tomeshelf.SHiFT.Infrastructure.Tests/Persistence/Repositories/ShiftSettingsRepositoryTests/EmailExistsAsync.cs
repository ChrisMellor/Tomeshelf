using FakeItEasy;
using Shouldly;
using Tomeshelf.SHiFT.Application.Abstractions.Security;
using Tomeshelf.SHiFT.Domain.Entities;
using Tomeshelf.SHiFT.Infrastructure.Persistence.Repositories;
using Tomeshelf.SHiFT.Infrastructure.Tests.TestUtilities;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Persistence.Repositories.ShiftSettingsRepositoryTests;

public class EmailExistsAsync
{
    [Fact]
    public async Task RespectsExcludingId()
    {
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

        var excluded = await repository.EmailExistsAsync("user@example.com", 5, CancellationToken.None);
        var included = await repository.EmailExistsAsync("user@example.com", 6, CancellationToken.None);

        excluded.ShouldBeFalse();
        included.ShouldBeTrue();
    }
}