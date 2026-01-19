using System;

namespace Tomeshelf.Application.Contracts.SHiFT;

/// <summary>
///     Represents the shift-related settings for a user, including contact information, default service, password status,
///     and the last update timestamp.
/// </summary>
/// <param name="Email">
///     The email address associated with the user. This is used for identification and communication
///     purposes.
/// </param>
/// <param name="DefaultService">
///     The name of the default service assigned to the user. This value determines which service is preselected or
///     prioritized for the user's shifts.
/// </param>
/// <param name="HasPassword">
///     A value indicating whether the user has a password set. Set to <see langword="true" /> if a password exists;
///     otherwise, <see langword="false" />.
/// </param>
/// <param name="UpdatedUtc">
///     The date and time, in Coordinated Universal Time (UTC), when the shift settings were last
///     updated.
/// </param>
public sealed record ShiftSettingsDto(string Email, string DefaultService, bool HasPassword, DateTimeOffset UpdatedUtc);