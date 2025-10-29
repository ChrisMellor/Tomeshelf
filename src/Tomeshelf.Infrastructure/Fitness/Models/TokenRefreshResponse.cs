using System.Text.Json.Serialization;

namespace Tomeshelf.Infrastructure.Fitness.Models;

public sealed class TokenRefreshResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; }

    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; init; }
}