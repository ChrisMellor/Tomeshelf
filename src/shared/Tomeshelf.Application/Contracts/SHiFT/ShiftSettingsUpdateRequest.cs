namespace Tomeshelf.Application.Shared.Contracts.SHiFT;

/// <summary>
///     Represents a request to update shift settings for a user, including authentication and default service information.
/// </summary>
/// <param name="Email">The email address of the user whose shift settings are being updated. Cannot be null or empty.</param>
/// <param name="Password">The new password for the user. Specify null to leave the password unchanged.</param>
/// <param name="DefaultService">The identifier of the default service to associate with the user. Cannot be null or empty.</param>
public sealed record ShiftSettingsUpdateRequest(string Email, string? Password, string DefaultService);