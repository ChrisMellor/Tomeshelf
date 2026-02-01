using Bogus;
using FluentAssertions;
using Tomeshelf.SHiFT.Domain.Entities;

namespace Tomeshelf.SHiFT.Domain.Tests.SettingsEntityTests;

public class UpdateSettings
{
    [Fact]
    public void UpdatesAllFieldsAndTimestamp()
    {
        // Arrange
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

        // Act
        settings.UpdateSettings(newEmail, newEncryptedPassword, newDefaultService);

        // Assert
        settings.Email.Should().Be(newEmail);
        settings.EncryptedPassword.Should().Be(newEncryptedPassword);
        settings.DefaultService.Should().Be(newDefaultService);
        settings.UpdatedUtc.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void WithSameValues_StillUpdatesTimestamp()
    {
        // Arrange
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

        // Act
        settings.UpdateSettings(email, encryptedPassword, defaultService);

        // Assert
        settings.Email.Should().Be(email);
        settings.EncryptedPassword.Should().Be(encryptedPassword);
        settings.DefaultService.Should().Be(defaultService);
        settings.UpdatedUtc.Should().BeAfter(initialUpdatedUtc);
    }
}
