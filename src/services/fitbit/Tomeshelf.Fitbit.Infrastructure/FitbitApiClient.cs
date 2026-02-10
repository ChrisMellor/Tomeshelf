using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tomeshelf.Fitbit.Application;
using Tomeshelf.Fitbit.Application.Exceptions;
using Tomeshelf.Fitbit.Infrastructure.Models;

namespace Tomeshelf.Fitbit.Infrastructure;

internal sealed class FitbitApiClient : IFitbitApiClient
{
    public const string HttpClientName = "Fitbit.ApiClient";
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

    private static readonly TimeSpan RefreshSkew = TimeSpan.FromMinutes(1);
    private static readonly SemaphoreSlim RefreshLock = new SemaphoreSlim(1, 1);

    private readonly HttpClient _httpClient;
    private readonly ILogger<FitbitApiClient> _logger;
    private readonly IOptionsMonitor<FitbitOptions> _options;
    private readonly FitbitTokenCache _tokenCache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FitbitApiClient" /> class.
    /// </summary>
    /// <param name="httpClientFactory">The http client factory.</param>
    /// <param name="options">The options.</param>
    /// <param name="tokenCache">The token cache.</param>
    /// <param name="logger">The logger.</param>
    public FitbitApiClient(IHttpClientFactory httpClientFactory, IOptionsMonitor<FitbitOptions> options, FitbitTokenCache tokenCache, ILogger<FitbitApiClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient(HttpClientName);
        _options = options;
        _tokenCache = tokenCache;
        _logger = logger;
    }

    /// <summary>
    ///     Gets the activities asynchronously.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public Task<ActivitiesResponse> GetActivitiesAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var path = $"1/user/{userId}/activities/date/{date:yyyy-MM-dd}.json";

        return GetJsonAsync<ActivitiesResponse>(path, cancellationToken);
    }

    /// <summary>
    ///     Gets the calories in asynchronously.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public Task<FoodLogSummaryResponse> GetCaloriesInAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var path = $"1/user/{userId}/foods/log/date/{date:yyyy-MM-dd}.json";

        return GetJsonAsync<FoodLogSummaryResponse>(path, cancellationToken);
    }

    /// <summary>
    ///     Gets the sleep asynchronously.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public Task<SleepResponse> GetSleepAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var path = $"1.2/user/{userId}/sleep/date/{date:yyyy-MM-dd}.json";

        return GetJsonAsync<SleepResponse>(path, cancellationToken);
    }

    /// <summary>
    ///     Gets the weight asynchronously.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="lookbackDays">The lookback days.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public Task<WeightResponse> GetWeightAsync(DateOnly date, int lookbackDays, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        lookbackDays = Math.Clamp(lookbackDays, 1, 365);
        var path = $"1/user/{userId}/body/log/weight/date/{date:yyyy-MM-dd}/{lookbackDays}d.json";

        return GetJsonAsync<WeightResponse>(path, cancellationToken);
    }

    /// <summary>
    ///     Ensures the access token asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    private async Task<bool> EnsureAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (!IsAccessTokenStale())
        {
            return true;
        }

        if (await TryRefreshTokenAsync(cancellationToken)
               .ConfigureAwait(false))
        {
            return !IsAccessTokenExpired();
        }

        return !IsAccessTokenExpired();
    }

    /// <summary>
    ///     Gets the json asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of t.</typeparam>
    /// <param name="path">The path.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    private async Task<T> GetJsonAsync<T>(string path, CancellationToken cancellationToken)
    {
        if (!await EnsureAccessTokenAsync(cancellationToken)
               .ConfigureAwait(false))
        {
            throw new InvalidOperationException("Fitbit OAuth credentials are not configured. Configure an initial access/refresh token pair.");
        }

        const int MaxAttempts = 5;
        var attempts = 0;

        while (true)
        {
            attempts++;
            var accessToken = _tokenCache.AccessToken;
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new InvalidOperationException("Fitbit OAuth credentials are not configured. Configure an initial access/refresh token pair.");
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.ParseAdd("application/json");

            var started = DateTimeOffset.UtcNow;
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                                                  .ConfigureAwait(false);
            var elapsed = DateTimeOffset.UtcNow - started;

            if ((response.StatusCode == HttpStatusCode.Unauthorized) &&
                (attempts < 3) &&
                await TryRefreshTokenAsync(cancellationToken, accessToken, true)
                   .ConfigureAwait(false))
            {
                accessToken = _tokenCache.AccessToken ?? accessToken;
                _logger.LogWarning("Fitbit API request to {Path} received 401; refreshed token and retrying (attempt {Attempt}).", path, attempts);

                continue;
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                var retryDelay = GetRetryAfterDelay(response);
                var attemptInfo = $"{attempts}/{MaxAttempts}";

                if (attempts >= MaxAttempts)
                {
                    var payload = await response.Content
                                                .ReadAsStringAsync(cancellationToken)
                                                .ConfigureAwait(false);

                    throw new FitbitRateLimitExceededException(payload, retryDelay);
                }

                var delay = retryDelay ?? TimeSpan.FromSeconds(30);
                _logger.LogWarning("Fitbit API rate limit hit for {Path}. Waiting {DelaySeconds}s before retrying (attempt {AttemptInfo}).", path, (int)delay.TotalSeconds, attemptInfo);
                await Task.Delay(delay, cancellationToken)
                          .ConfigureAwait(false);

                continue;
            }

            _logger.LogInformation("Fitbit API {Method} {Path} -> {Status} in {Duration}ms", request.Method, path, (int)response.StatusCode, (int)elapsed.TotalMilliseconds);

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var payload = await response.Content
                                            .ReadAsStringAsync(cancellationToken)
                                            .ConfigureAwait(false);

                throw new FitbitBadRequestException(payload);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content
                                                   .ReadAsStreamAsync(cancellationToken)
                                                   .ConfigureAwait(false);

            return await JsonSerializer.DeserializeAsync<T>(stream, SerializerOptions, cancellationToken)
                                       .ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Gets the retry after delay.
    /// </summary>
    /// <param name="response">The response.</param>
    /// <returns>The result of the operation.</returns>
    private static TimeSpan? GetRetryAfterDelay(HttpResponseMessage response)
    {
        if (response.Headers.RetryAfter is
            {
                Delta:
                { } delta
            })
        {
            return delta > TimeSpan.Zero
                ? delta
                : TimeSpan.Zero;
        }

        if (response.Headers.RetryAfter?.Date is
            { } retryDate)
        {
            var delay = retryDate - DateTimeOffset.UtcNow;

            return delay > TimeSpan.Zero
                ? delay
                : TimeSpan.Zero;
        }

        if (response.Headers.TryGetValues("Retry-After", out var values))
        {
            var first = values.FirstOrDefault();
            if (double.TryParse(first, NumberStyles.Any, CultureInfo.InvariantCulture, out var seconds) && (seconds > 0))
            {
                return TimeSpan.FromSeconds(seconds);
            }
        }

        return null;
    }

    /// <summary>
    ///     Gets the user id.
    /// </summary>
    /// <returns>The resulting string.</returns>
    private string GetUserId()
    {
        var userId = _options.CurrentValue.UserId;

        return string.IsNullOrWhiteSpace(userId)
            ? "-"
            : Uri.EscapeDataString(userId);
    }

    /// <summary>
    ///     Determines whether the access token is expired.
    /// </summary>
    /// <returns>True if the condition is met; otherwise, false.</returns>
    private bool IsAccessTokenExpired()
    {
        if (!TryGetAccessToken(out _))
        {
            return true;
        }

        var expiresAt = _tokenCache.ExpiresAtUtc;
        if (!expiresAt.HasValue)
        {
            return false;
        }

        return expiresAt.Value <= DateTimeOffset.UtcNow;
    }

    /// <summary>
    ///     Determines whether the access token is stale.
    /// </summary>
    /// <returns>True if the condition is met; otherwise, false.</returns>
    private bool IsAccessTokenStale()
    {
        if (!TryGetAccessToken(out _))
        {
            return true;
        }

        var expiresAt = _tokenCache.ExpiresAtUtc;
        if (!expiresAt.HasValue)
        {
            return false;
        }

        return expiresAt.Value <= DateTimeOffset.UtcNow.Add(RefreshSkew);
    }

    /// <summary>
    ///     Attempts to get an access token.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>True if the condition is met; otherwise, false.</returns>
    private bool TryGetAccessToken(out string token)
    {
        var cached = _tokenCache.AccessToken;

        if (string.IsNullOrWhiteSpace(cached))
        {
            token = string.Empty;

            return false;
        }

        token = cached;

        return true;
    }

    /// <summary>
    ///     Attempts to refresh a token asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="failedAccessToken">The failed access token.</param>
    /// <param name="force">The force.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    private async Task<bool> TryRefreshTokenAsync(CancellationToken cancellationToken, string failedAccessToken = null, bool force = false)
    {
        await RefreshLock.WaitAsync(cancellationToken)
                         .ConfigureAwait(false);

        try
        {
            if (!string.IsNullOrWhiteSpace(failedAccessToken))
            {
                var current = _tokenCache.AccessToken;
                if (!string.IsNullOrWhiteSpace(current) && !string.Equals(current, failedAccessToken, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            if (!force && !IsAccessTokenStale())
            {
                return true;
            }

            var refreshToken = _tokenCache.RefreshToken;
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                _logger.LogWarning("Fitbit refresh token is not configured; cannot refresh access token.");

                return false;
            }

            var options = _options.CurrentValue;

            using var request = new HttpRequestMessage(HttpMethod.Post, "oauth2/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = refreshToken
                })
            };

            var credentials = $"{options.ClientId}:{options.ClientSecret}";
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials)));
            request.Headers.Accept.ParseAdd("application/json");

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                                                  .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to refresh Fitbit access token. Status: {StatusCode}", response.StatusCode);

                return false;
            }

            await using var stream = await response.Content
                                                   .ReadAsStreamAsync(cancellationToken)
                                                   .ConfigureAwait(false);
            var payload = await JsonSerializer.DeserializeAsync<TokenRefreshResponse>(stream, SerializerOptions, cancellationToken)
                                              .ConfigureAwait(false);

            if (payload?.AccessToken is null)
            {
                _logger.LogWarning("Fitbit token refresh response was missing an access token.");

                return false;
            }

            var newRefreshToken = !string.IsNullOrWhiteSpace(payload.RefreshToken)
                ? payload.RefreshToken
                : refreshToken;

            DateTimeOffset? expiresAt = payload.ExpiresIn.HasValue
                ? DateTimeOffset.UtcNow.AddSeconds(payload.ExpiresIn.Value)
                : null;

            _tokenCache.Update(payload.AccessToken, newRefreshToken, expiresAt);

            return true;
        }
        finally
        {
            RefreshLock.Release();
        }
    }
}