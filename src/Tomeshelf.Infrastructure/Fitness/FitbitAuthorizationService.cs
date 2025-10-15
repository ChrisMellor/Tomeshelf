#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tomeshelf.Application.Options;
using Tomeshelf.Infrastructure.Fitness.Models;

namespace Tomeshelf.Infrastructure.Fitness;

public sealed class FitbitAuthorizationService
{
    private static readonly TimeSpan StateLifetime = TimeSpan.FromMinutes(10);
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FitbitAuthorizationService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IOptionsMonitor<FitbitOptions> _options;
    private readonly FitbitTokenCache _tokenCache;

    public FitbitAuthorizationService(IHttpClientFactory httpClientFactory, FitbitTokenCache tokenCache, IMemoryCache memoryCache, ILogger<FitbitAuthorizationService> logger, IOptionsMonitor<FitbitOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _tokenCache = tokenCache;
        _memoryCache = memoryCache;
        _logger = logger;
        _options = options;
    }

    public Uri BuildAuthorizationUri(string? returnUrl, out string state)
    {
        var options = _options.CurrentValue;
        state = Guid.NewGuid()
                    .ToString("N");
        var codeVerifier = CreateCodeVerifier();
        var targetReturnUrl = string.IsNullOrWhiteSpace(returnUrl)
                ? "/fitness"
                : returnUrl!;
        var authState = new AuthorizationState(codeVerifier, targetReturnUrl);
        _memoryCache.Set(GetStateCacheKey(state), authState, StateLifetime);

        var callback = BuildCallbackUri(options);
        var codeChallenge = CreateCodeChallenge(codeVerifier);

        var query = new Dictionary<string, string?>
        {
                ["response_type"] = "code",
                ["client_id"] = options.ClientId,
                ["redirect_uri"] = callback.ToString(),
                ["scope"] = options.Scope,
                ["state"] = state,
                ["prompt"] = "login",
                ["code_challenge"] = codeChallenge,
                ["code_challenge_method"] = "S256"
        };

        var builder = new StringBuilder("https://www.fitbit.com/oauth2/authorize?");
        var encoder = UrlEncoder.Default;
        var first = true;
        foreach (var kvp in query)
        {
            if (!first)
            {
                builder.Append('&');
            }

            builder.Append(encoder.Encode(kvp.Key));
            builder.Append('=');
            builder.Append(encoder.Encode(kvp.Value ?? string.Empty));
            first = false;
        }

        return new Uri(builder.ToString());
    }

    public bool TryConsumeState(string state, out string codeVerifier, out string returnUrl)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            codeVerifier = string.Empty;
            returnUrl = "/fitness";

            return false;
        }

        var cacheKey = GetStateCacheKey(state);
        if (_memoryCache.TryGetValue(cacheKey, out var stateObj) && stateObj is AuthorizationState stored)
        {
            _memoryCache.Remove(cacheKey);
            codeVerifier = stored.CodeVerifier;
            returnUrl = stored.ReturnUrl;

            return true;
        }

        codeVerifier = string.Empty;
        returnUrl = "/fitness";

        return false;
    }

    public async Task ExchangeAuthorizationCodeAsync(string code, string codeVerifier, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(codeVerifier))
        {
            throw new InvalidOperationException("Missing PKCE code verifier for Fitbit authorization exchange.");
        }

        var options = _options.CurrentValue;

        if (string.IsNullOrWhiteSpace(options.ClientId) || string.IsNullOrWhiteSpace(options.ClientSecret))
        {
            throw new InvalidOperationException("Fitbit client credentials are not configured.");
        }

        var callback = BuildCallbackUri(options);

        var client = _httpClientFactory.CreateClient("FitbitOAuth");
        using var request = new HttpRequestMessage(HttpMethod.Post, "oauth2/token")
        {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                        ["grant_type"] = "authorization_code",
                        ["code"] = code,
                        ["redirect_uri"] = callback.ToString(),
                        ["code_verifier"] = codeVerifier
                })
        };

        var credentials = $"{options.ClientId}:{options.ClientSecret}";
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials)));
        request.Headers.Accept.ParseAdd("application/json");

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                                         .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
                                               .ConfigureAwait(false);
        var payload = await JsonSerializer.DeserializeAsync<TokenRefreshResponse>(stream, cancellationToken: cancellationToken)
                                          .ConfigureAwait(false);

        if (payload?.AccessToken is null)
        {
            throw new InvalidOperationException("Fitbit token response was missing an access token.");
        }

        DateTimeOffset? expiresAt = payload.ExpiresIn.HasValue
                ? DateTimeOffset.UtcNow.AddSeconds(payload.ExpiresIn.Value)
                : null;

        _tokenCache.Update(payload.AccessToken, payload.RefreshToken, expiresAt);
        _logger.LogInformation("Successfully obtained Fitbit access token via authorization code grant.");
    }

    private static string GetStateCacheKey(string state)
    {
        return $"fitbit:oauth:state:{state}";
    }

    private static string CreateCodeVerifier()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);

        return Base64UrlEncode(bytes);
    }

    private static string CreateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));

        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
                      .TrimEnd('=')
                      .Replace('+', '-')
                      .Replace('/', '_');
    }

    private static Uri BuildCallbackUri(FitbitOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.CallbackBaseUri))
        {
            throw new InvalidOperationException("Fitbit CallbackBaseUri must be configured.");
        }

        if (!Uri.TryCreate(options.CallbackBaseUri, UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException($"Invalid Fitbit CallbackBaseUri '{options.CallbackBaseUri}'.");
        }

        var path = string.IsNullOrWhiteSpace(options.CallbackPath)
                ? "/api/fitbit/auth/callback"
                : options.CallbackPath;

        if (!path.StartsWith('/'))
        {
            path = "/" + path;
        }

        return new Uri(baseUri, path);
    }

    private sealed record AuthorizationState(string CodeVerifier, string ReturnUrl);
}