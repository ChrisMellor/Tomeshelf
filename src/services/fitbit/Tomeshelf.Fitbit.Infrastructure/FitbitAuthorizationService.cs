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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tomeshelf.Fitbit.Application;
using Tomeshelf.Fitbit.Application.Abstractions.Services;
using Tomeshelf.Fitbit.Infrastructure.Models;

namespace Tomeshelf.Fitbit.Infrastructure;

public sealed class FitbitAuthorizationService : IFitbitAuthorizationService
{
    public const string HttpClientName = "Fitbit.OAuth";
    private static readonly TimeSpan StateLifetime = TimeSpan.FromMinutes(10);
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<FitbitAuthorizationService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IOptionsMonitor<FitbitOptions> _options;
    private readonly FitbitTokenCache _tokenCache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FitbitAuthorizationService" /> class.
    /// </summary>
    /// <param name="httpClientFactory">The http client factory.</param>
    /// <param name="tokenCache">The token cache.</param>
    /// <param name="memoryCache">The memory cache.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The options.</param>
    /// <param name="httpContextAccessor">The http context accessor.</param>
    public FitbitAuthorizationService(IHttpClientFactory httpClientFactory, FitbitTokenCache tokenCache, IMemoryCache memoryCache, ILogger<FitbitAuthorizationService> logger, IOptionsMonitor<FitbitOptions> options, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _tokenCache = tokenCache ?? throw new ArgumentNullException(nameof(tokenCache));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <summary>
    ///     Builds the authorization uri.
    /// </summary>
    /// <param name="returnUrl">The return url.</param>
    /// <param name="state">The state.</param>
    /// <returns>The result of the operation.</returns>
    public Uri BuildAuthorizationUri(string returnUrl, out string state)
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

        var callback = BuildCallbackUri(options, _httpContextAccessor.HttpContext?.Request);
        var codeChallenge = CreateCodeChallenge(codeVerifier);

        var query = new Dictionary<string, string>
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

    /// <summary>
    ///     Exchanges the authorization code asynchronously.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <param name="codeVerifier">The code verifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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

        var callback = BuildCallbackUri(options, _httpContextAccessor.HttpContext?.Request);

        var client = _httpClientFactory.CreateClient(HttpClientName);
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

        await using var stream = await response.Content
                                               .ReadAsStreamAsync(cancellationToken)
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

    /// <summary>
    ///     Attempts to consume a state.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="codeVerifier">The code verifier.</param>
    /// <param name="returnUrl">The return url.</param>
    /// <returns>True if the condition is met; otherwise, false.</returns>
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

    /// <summary>
    ///     Builds the callback uri.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="request">The request.</param>
    /// <returns>The result of the operation.</returns>
    public static Uri BuildCallbackUri(FitbitOptions options, HttpRequest request)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var path = string.IsNullOrWhiteSpace(options.CallbackPath) ? new PathString("/api/fitbit/auth/callback") :
            options.CallbackPath.StartsWith("/", StringComparison.Ordinal) ? new PathString(options.CallbackPath) : new PathString("/" + options.CallbackPath);

        Uri baseUri;

        if (string.IsNullOrWhiteSpace(options.CallbackBaseUri))
        {
            if (request is null)
            {
                throw new InvalidOperationException("Fitbit CallbackBaseUri is not configured and no HTTP request is available to infer it.");
            }

            if (string.IsNullOrWhiteSpace(request.Scheme))
            {
                throw new InvalidOperationException("Cannot infer Fitbit callback URI because the current HTTP request has no scheme.");
            }

            if (!request.Host.HasValue)
            {
                throw new InvalidOperationException("Cannot infer Fitbit callback URI because the current HTTP request has no host.");
            }

            baseUri = new Uri($"{request.Scheme}://{request.Host.ToUriComponent()}");

            if (request.PathBase.HasValue)
            {
                path = request.PathBase.Add(path);
            }
        }
        else if (!Uri.TryCreate(options.CallbackBaseUri, UriKind.Absolute, out baseUri))
        {
            throw new InvalidOperationException($"Invalid Fitbit CallbackBaseUri '{options.CallbackBaseUri}'.");
        }
        else if (request?.PathBase.HasValue == true)
        {
            path = request.PathBase.Add(path);
        }

        var pathValue = path.HasValue
            ? path.Value
            : "/api/fitbit/auth/callback";

        if ((pathValue.Length == 0) || (pathValue[0] != '/'))
        {
            pathValue = "/" + pathValue;
        }

        return new Uri(baseUri, pathValue);
    }

    /// <summary>
    ///     Bases the 64 url encode.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns>The resulting string.</returns>
    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
                      .TrimEnd('=')
                      .Replace('+', '-')
                      .Replace('/', '_');
    }

    /// <summary>
    ///     Creates the code challenge.
    /// </summary>
    /// <param name="codeVerifier">The code verifier.</param>
    /// <returns>The resulting string.</returns>
    private static string CreateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));

        return Base64UrlEncode(hash);
    }

    /// <summary>
    ///     Creates the code verifier.
    /// </summary>
    /// <returns>The resulting string.</returns>
    private static string CreateCodeVerifier()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);

        return Base64UrlEncode(bytes);
    }

    /// <summary>
    ///     Gets the state cache key.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <returns>The resulting string.</returns>
    private static string GetStateCacheKey(string state)
    {
        return $"fitbit:oauth:state:{state}";
    }
}