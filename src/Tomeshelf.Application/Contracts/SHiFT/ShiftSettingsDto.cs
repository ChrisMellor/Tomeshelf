using System;

namespace Tomeshelf.SHiFT.Api;

public sealed record ShiftSettingsDto(string Email, string DefaultService, bool HasPassword, DateTimeOffset UpdatedUtc);