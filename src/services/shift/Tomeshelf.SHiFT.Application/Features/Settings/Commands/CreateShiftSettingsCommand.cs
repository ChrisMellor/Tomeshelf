using Tomeshelf.Application.Shared.Abstractions.Messaging;

namespace Tomeshelf.SHiFT.Application.Features.Settings.Commands;

/// <summary>
///     Represents a command to create shift settings for a user, including their email address, optional password, and
///     default service preference.
/// </summary>
/// <remarks>
///     Use this command to initialize shift settings for a user. Ensure that the provided email and default
///     service are valid and correspond to existing user and service records.
/// </remarks>
/// <param name="Email">
///     The email address of the user for whom the shift settings are being created. This parameter is required and cannot
///     be null.
/// </param>
/// <param name="Password">The password associated with the user's account. This parameter is optional and can be null.</param>
/// <param name="DefaultService">
///     The default service to be assigned for the user's shift settings. This parameter is
///     required and cannot be null.
/// </param>
public sealed record CreateShiftSettingsCommand(string Email, string? Password, string DefaultService) : ICommand<int>;