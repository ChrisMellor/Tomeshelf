namespace Tomeshelf.Application.Contracts.SHiFT;

public sealed record ShiftSettingsUpdateRequest(string Email, string? Password, string DefaultService);