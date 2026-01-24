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
}