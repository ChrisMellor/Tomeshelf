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
using Tomeshelf.Application.Options;
using Tomeshelf.Infrastructure.Fitness.Models;

namespace Tomeshelf.Infrastructure.Fitness;

internal sealed class FitbitApiClient : IFitbitApiClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient;
    private readonly ILogger<FitbitApiClient> _logger;
    private readonly IOptionsMonitor<FitbitOptions> _options;
    private readonly FitbitTokenCache _tokenCache;

    public FitbitApiClient(HttpClient httpClient, IOptionsMonitor<FitbitOptions> options, FitbitTokenCache tokenCache, ILogger<FitbitApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _tokenCache = tokenCache;
        _logger = logger;
    }

    public Task<ActivitiesResponse> GetActivitiesAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var path = $"1/user/{userId}/activities/date/{date:yyyy-MM-dd}.json";

        return GetJsonAsync<ActivitiesResponse>(path, cancellationToken);
    }

    public Task<FoodLogSummaryResponse> GetCaloriesInAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var path = $"1/user/{userId}/foods/log/date/{date:yyyy-MM-dd}.json";

        return GetJsonAsync<FoodLogSummaryResponse>(path, cancellationToken);
    }

    public Task<SleepResponse> GetSleepAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var path = $"1.2/user/{userId}/sleep/date/{date:yyyy-MM-dd}.json";

        return GetJsonAsync<SleepResponse>(path, cancellationToken);
    }

    public Task<WeightResponse> GetWeightAsync(DateOnly date, int lookbackDays, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        lookbackDays = Math.Clamp(lookbackDays, 1, 365);
        var path = $"1/user/{userId}/body/log/weight/date/{date:yyyy-MM-dd}/{lookbackDays}d.json";

        return GetJsonAsync<WeightResponse>(path, cancellationToken);
    }

    private string GetUserId()
    {
        var userId = _options.CurrentValue.UserId;

        return string.IsNullOrWhiteSpace(userId)
                ? "-"
                : Uri.EscapeDataString(userId);
    }

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
                await TryRefreshTokenAsync(cancellationToken)
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
                    var payload = await response.Content.ReadAsStringAsync(cancellationToken)
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
                var payload = await response.Content.ReadAsStringAsync(cancellationToken)
                                            .ConfigureAwait(false);

                throw new FitbitBadRequestException(payload);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
                                                   .ConfigureAwait(false);

            return await JsonSerializer.DeserializeAsync<T>(stream, SerializerOptions, cancellationToken)
                                       .ConfigureAwait(false);
        }
    }

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

    private async Task<bool> EnsureAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (_tokenCache.ExpiresAtUtc is
                    { } expiresAt &&
            (expiresAt <= DateTimeOffset.UtcNow.AddMinutes(-1)))
        {
            await TryRefreshTokenAsync(cancellationToken)
                   .ConfigureAwait(false);
        }

        if (TryGetAccessToken(out _))
        {
            return true;
        }

        if (await TryRefreshTokenAsync(cancellationToken)
                   .ConfigureAwait(false))
        {
            return TryGetAccessToken(out _);
        }

        return false;
    }

    private async Task<bool> TryRefreshTokenAsync(CancellationToken cancellationToken)
    {
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

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
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
}

public sealed class FitbitRateLimitExceededException : Exception
{
    public FitbitRateLimitExceededException(string rawMessage, TimeSpan? retryAfter) : base(BuildMessage(rawMessage))
    {
        RetryAfter = retryAfter;
    }

    public TimeSpan? RetryAfter { get; }

    private static string BuildMessage(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "Fitbit rate limit reached. Please try again shortly.";
        }

        var trimmed = raw.Trim();

        if (trimmed.StartsWith("{", StringComparison.Ordinal))
        {
            try
            {
                using var document = JsonDocument.Parse(trimmed);
                if ((document.RootElement.ValueKind == JsonValueKind.Object) && document.RootElement.TryGetProperty("message", out var messageElement) && (messageElement.ValueKind == JsonValueKind.String))
                {
                    var message = messageElement.GetString();
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        return message.Trim();
                    }
                }
            }
            catch (JsonException)
            {
                // ignore malformed json
            }
        }

        if (trimmed.StartsWith("\"", StringComparison.Ordinal) && trimmed.EndsWith("\"", StringComparison.Ordinal))
        {
            try
            {
                var deserialised = JsonSerializer.Deserialize<string>(trimmed);
                if (!string.IsNullOrWhiteSpace(deserialised))
                {
                    return deserialised.Trim();
                }
            }
            catch (JsonException)
            {
                // ignore malformed json
            }
        }

        if (trimmed.StartsWith("<", StringComparison.Ordinal))
        {
            return "Fitbit rate limit reached. Please try again shortly.";
        }

        return trimmed;
    }
}

public sealed class FitbitBadRequestException : Exception
{
    public FitbitBadRequestException(string rawMessage) : base(BuildMessage(rawMessage)) { }

    private static string BuildMessage(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "Fitbit rejected the request. Please re-authorize and try again.";
        }

        var trimmed = raw.Trim();

        if (trimmed.StartsWith("{", StringComparison.Ordinal))
        {
            try
            {
                using var document = JsonDocument.Parse(trimmed);
                if ((document.RootElement.ValueKind == JsonValueKind.Object) && document.RootElement.TryGetProperty("message", out var messageElement) && (messageElement.ValueKind == JsonValueKind.String))
                {
                    var message = messageElement.GetString();
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        return message.Trim();
                    }
                }
            }
            catch (JsonException)
            {
                // ignore malformed json
            }
        }

        if (trimmed.StartsWith("\"", StringComparison.Ordinal) && trimmed.EndsWith("\"", StringComparison.Ordinal))
        {
            try
            {
                var deserialised = JsonSerializer.Deserialize<string>(trimmed);
                if (!string.IsNullOrWhiteSpace(deserialised))
                {
                    return deserialised.Trim();
                }
            }
            catch (JsonException)
            {
                // ignore malformed json
            }
        }

        if (trimmed.StartsWith("<", StringComparison.Ordinal))
        {
            return "Fitbit could not process the request. Please re-authorize and try again.";
        }

        return trimmed;
    }
}