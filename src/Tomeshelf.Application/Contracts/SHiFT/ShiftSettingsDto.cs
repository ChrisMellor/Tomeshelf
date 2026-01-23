using System;

namespace Tomeshelf.Application.Contracts.SHiFT;

/// <summary>
///     Represents the shift settings for a user, including identification, contact information, default service, password
///     status, and the last update timestamp.
/// </summary>
/// <param name="Id">The unique identifier for the shift settings record.</param>
/// <param name="Email">The email address associated with the user whose shift settings are represented.</param>
/// <param name="DefaultService">The name of the default service assigned to the user for their shift.</param>
/// <param name="HasPassword">
///     Indicates whether the user has a password set. <see langword="true" /> if a password exists; otherwise,
///     <see langword="false" />.
/// </param>
/// <param name="UpdatedUtc">
///     The date and time, in Coordinated Universal Time (UTC), when the shift settings were last
///     updated.
/// </param>
public sealed record ShiftSettingsDto(int Id, string Email, string DefaultService, bool HasPassword, DateTimeOffset UpdatedUtc);