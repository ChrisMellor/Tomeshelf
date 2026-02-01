using System;
using Xunit;
using Tomeshelf.SHiFT.Domain.Entities;

namespace Tomeshelf.SHiFT.Domain.Tests;

public class SettingsEntityTests
{
    [Fact]
    public void UpdateSettings_UpdatesAllFieldsAndTimestamp()
    {
        // Arrange
        var settings = new SettingsEntity
        {
            Id = 1,
            Email = "old@example.com",
            EncryptedPassword = "old_password",
            DefaultService = "old_service",
            UpdatedUtc = DateTimeOffset.UtcNow.AddDays(-1)
        };

        var newEmail = "new@example.com";
        var newEncryptedPassword = "new_password";
        var newDefaultService = "new_service";

        // Act
        settings.UpdateSettings(newEmail, newEncryptedPassword, newDefaultService);

        // Assert
        Assert.Equal(newEmail, settings.Email);
        Assert.Equal(newEncryptedPassword, settings.EncryptedPassword);
        Assert.Equal(newDefaultService, settings.DefaultService);
        Assert.True(settings.UpdatedUtc > DateTimeOffset.UtcNow.AddMinutes(-1)); // Should be very recent
    }

    [Fact]
    public void UpdateSettings_WithSameValues_StillUpdatesTimestamp()
    {
        // Arrange
        var initialUpdatedUtc = DateTimeOffset.UtcNow.AddDays(-1);
        var settings = new SettingsEntity
        {
            Id = 1,
            Email = "test@example.com",
            EncryptedPassword = "password",
            DefaultService = "psn",
            UpdatedUtc = initialUpdatedUtc
        };

        var email = "test@example.com";
        var encryptedPassword = "password";
        var defaultService = "psn";

        // Act
        settings.UpdateSettings(email, encryptedPassword, defaultService);

        // Assert
        Assert.Equal(email, settings.Email);
        Assert.Equal(encryptedPassword, settings.EncryptedPassword);
        Assert.Equal(defaultService, settings.DefaultService);
        Assert.True(settings.UpdatedUtc > initialUpdatedUtc); // Timestamp should still be updated
    }
}
