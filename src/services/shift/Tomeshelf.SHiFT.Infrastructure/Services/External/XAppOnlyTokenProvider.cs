using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tomeshelf.SHiFT.Application;

namespace Tomeshelf.SHiFT.Infrastructure.Services.External;

public sealed class XAppOnlyTokenProvider
{
    public const string HttpClientName = "Shift.XAuth";

    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<XAppOnlyTokenProvider> _logger;
    private readonly IOptionsMonitor<ShiftKeyScannerOptions> _options;
    private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);
    private CachedToken? _cached;

    public XAppOnlyTokenProvider(IHttpClientFactory httpClientFactory, IOptionsMonitor<ShiftKeyScannerOptions> options, ILogger<XAppOnlyTokenProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    public async Task<string?> GetBearerTokenAsync(CancellationToken cancellationToken)
    {
        var settings = _options.CurrentValue?.X;
        if (settings is null || !settings.Enabled)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(settings.BearerToken))
        {
            return settings.BearerToken.Trim();
        }

        if (string.IsNullOrWhiteSpace(settings.ApiKey) || string.IsNullOrWhiteSpace(settings.ApiSecret))
        {
            _logger.LogWarning("X app-only token requested but ApiKey/ApiSecret are not configured.");

            return null;
        }

        if (string.IsNullOrWhiteSpace(settings.OAuthTokenEndpoint))
        {
            _logger.LogWarning("X app-only token requested but OAuthTokenEndpoint is not configured.");

            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var cacheWindow = TimeSpan.FromMinutes(Math.Clamp(settings.TokenCacheMinutes, 1, 1440));

        if (_cached is
                { } cached &&
            (cached.ExpiresUtc > now))
        {
            return cached.AccessToken;
        }

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (_cached is
                    { } refreshed &&
                (refreshed.ExpiresUtc > now))
            {
                return refreshed.AccessToken;
            }

            var token = await RequestTokenAsync(settings, cancellationToken);
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            _cached = new CachedToken(token, now.Add(cacheWindow));

            return token;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public void Invalidate()
    {
        _cached = null;
    }

    private async Task<string?> RequestTokenAsync(ShiftKeyScannerOptions.XSourceOptions settings, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);

        using var request = new HttpRequestMessage(HttpMethod.Post, settings.OAuthTokenEndpoint);
        request.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") });

        var credentials = $"{settings.ApiKey}:{settings.ApiSecret}";
        var encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encoded);
        request.Headers.Accept.ParseAdd("application/json");

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("X app-only token request failed with status {StatusCode}.", (int)response.StatusCode);

            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<XTokenResponse>(stream, SerializerOptions, cancellationToken);
        var token = payload?.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("X app-only token response did not contain an access token.");
        }

        return token;
    }

    private sealed record CachedToken(string AccessToken, DateTimeOffset ExpiresUtc);

    private sealed record XTokenResponse([property: JsonPropertyName("token_type")] string? TokenType, [property: JsonPropertyName("access_token")] string? AccessToken);
}