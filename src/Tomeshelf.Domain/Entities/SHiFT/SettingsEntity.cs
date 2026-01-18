using System;

namespace Tomeshelf.Domain.Entities.SHiFT;

public sealed class SettingsEntity
{
    public int Id { get; set; } = 1;

    public string Email { get; set; } = "";

    public string EncryptedPassword { get; set; } = "";

    public string DefaultService { get; set; } = "psn";

    public DateTimeOffset UpdatedUtc { get; set; } = DateTimeOffset.UtcNow;
}