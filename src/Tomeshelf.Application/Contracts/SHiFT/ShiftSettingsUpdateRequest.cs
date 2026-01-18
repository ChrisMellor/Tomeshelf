namespace Tomeshelf.SHiFT.Api;

public sealed record ShiftSettingsUpdateRequest(string Email, string? Password, string DefaultService);