using System.Text.Json.Serialization;

namespace Tomeshelf.Web.Payloads;

public sealed class TokenResponsePayload
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; }

    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; init; }

    [JsonPropertyName("error")]
    public string Error { get; init; }
}