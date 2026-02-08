using System;

namespace Tomeshelf.SHiFT.Domain.Entities;

/// <summary>
///     Represents a user settings record containing authentication and service configuration information.
/// </summary>
public sealed class SettingsEntity
{
    /// <summary>
    ///     Gets or sets the unique identifier for the entity.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the email address associated with the entity.
    /// </summary>
    public string Email { get; set; } = "";

    /// <summary>
    ///     Gets or sets the password in its encrypted form.
    /// </summary>
    public string EncryptedPassword { get; set; } = "";

    /// <summary>
    ///     Gets or sets the default service identifier used for operations that require a service name.
    /// </summary>
    public string DefaultService { get; set; } = "psn";

    /// <summary>
    ///     Gets or sets the date and time, in Coordinated Universal Time (UTC), when the entity was last updated.
    /// </summary>
    public DateTimeOffset UpdatedUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Updates the user settings with new values and updates the last updated timestamp.
    /// </summary>
    /// <param name="email">The new email address.</param>
    /// <param name="encryptedPassword">The new encrypted password.</param>
    /// <param name="defaultService">The new default service identifier.</param>
    public void UpdateSettings(string email, string encryptedPassword, string defaultService)
    {
        Email = email;
        EncryptedPassword = encryptedPassword;
        DefaultService = defaultService;
        UpdatedUtc = DateTimeOffset.UtcNow;
    }
}