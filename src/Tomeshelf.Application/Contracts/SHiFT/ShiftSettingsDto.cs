using System;

namespace Tomeshelf.Application.Contracts.SHiFT;

public sealed record ShiftSettingsDto(string Email, string DefaultService, bool HasPassword, DateTimeOffset UpdatedUtc);