using Tomeshelf.Application.Shared.Abstractions.Messaging;

namespace Tomeshelf.SHiFT.Application.Features.Settings.Commands;

/// <summary>
///     Represents a command to update the shift settings for a specific user.
/// </summary>
/// <remarks>
///     Ensure that the provided email is associated with a valid user account. Authentication requirements
///     may vary depending on application security policies.
/// </remarks>
/// <param name="Id">The unique identifier of the user whose shift settings are to be updated.</param>
/// <param name="Email">
///     The email address of the user whose shift settings are to be updated. This value is required and must correspond to
///     an existing user account.
/// </param>
/// <param name="Password">
///     The password of the user. This value is optional and can be null if authentication is not
///     required for the update.
/// </param>
/// <param name="DefaultService">The default service to assign to the user's shift settings. This value is required.</param>
public sealed record UpdateShiftSettingsCommand(int Id, string Email, string? Password, string DefaultService) : ICommand<bool>;