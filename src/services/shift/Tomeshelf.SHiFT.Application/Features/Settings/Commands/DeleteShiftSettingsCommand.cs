using Tomeshelf.Application.Shared.Abstractions.Messaging;

namespace Tomeshelf.SHiFT.Application.Features.Settings.Commands;

/// <summary>
///     Represents a command to delete shift settings for a specific user.
/// </summary>
/// <param name="Id">The unique identifier of the user whose shift settings are to be deleted.</param>
public sealed record DeleteShiftSettingsCommand(int Id) : ICommand<bool>;