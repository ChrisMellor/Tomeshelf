using System;
using System.Text.Json.Serialization;

namespace Tomeshelf.Web.Models.Shift;

public sealed record ShiftAccountModel
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    [JsonPropertyName("defaultService")]
    public string DefaultService { get; init; } = string.Empty;

    [JsonPropertyName("hasPassword")]
    public bool HasPassword { get; init; }

    [JsonPropertyName("updatedUtc")]
    public DateTimeOffset UpdatedUtc { get; init; }
}

