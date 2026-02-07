using Bogus;
using Shouldly;
using Tomeshelf.SHiFT.Domain.Entities;

namespace Tomeshelf.SHiFT.Domain.Tests.SettingsEntityTests;

public class UpdateSettings
{
    [Fact]
    public void UpdatesAllFieldsAndTimestamp()
    {
        var faker = new Faker();
        var settings = new SettingsEntity
        {
            Id = 1,
            Email = faker.Internet.Email(),
            EncryptedPassword = faker.Random.AlphaNumeric(12),
            DefaultService = faker.Random.Word(),
            UpdatedUtc = DateTimeOffset.UtcNow.AddDays(-1)
        };

        var newEmail = faker.Internet.Email();
        var newEncryptedPassword = faker.Random.AlphaNumeric(12);
        var newDefaultService = faker.Random.Word();

        var beforeUpdate = DateTimeOffset.UtcNow;
        settings.UpdateSettings(newEmail, newEncryptedPassword, newDefaultService);

        settings.Email.ShouldBe(newEmail);
        settings.EncryptedPassword.ShouldBe(newEncryptedPassword);
        settings.DefaultService.ShouldBe(newDefaultService);
        (settings.UpdatedUtc >= beforeUpdate).ShouldBeTrue();
    }

    [Fact]
    public void WithSameValues_StillUpdatesTimestamp()
    {
        var faker = new Faker();
        var initialUpdatedUtc = DateTimeOffset.UtcNow.AddDays(-1);
        var email = faker.Internet.Email();
        var encryptedPassword = faker.Random.AlphaNumeric(12);
        var defaultService = faker.Random.Word();
        var settings = new SettingsEntity
        {
            Id = 1,
            Email = email,
            EncryptedPassword = encryptedPassword,
            DefaultService = defaultService,
            UpdatedUtc = initialUpdatedUtc
        };

        var beforeUpdate = DateTimeOffset.UtcNow;
        settings.UpdateSettings(email, encryptedPassword, defaultService);

        settings.Email.ShouldBe(email);
        settings.EncryptedPassword.ShouldBe(encryptedPassword);
        settings.DefaultService.ShouldBe(defaultService);
        (settings.UpdatedUtc >= beforeUpdate).ShouldBeTrue();
    }
}