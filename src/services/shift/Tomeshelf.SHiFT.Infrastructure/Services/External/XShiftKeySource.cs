using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.SHiFT.Application;
using Tomeshelf.SHiFT.Application.Abstractions.External;
using Tomeshelf.SHiFT.Application.Features.KeyDiscovery;
using Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Models;

namespace Tomeshelf.SHiFT.Infrastructure.Services.External;

public sealed class XShiftKeySource : IShiftKeySource
{
    public const string HttpClientName = "Shift.XApi";

    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<XShiftKeySource> _logger;
    private readonly IOptionsMonitor<ShiftKeyScannerOptions> _options;
    private readonly XAppOnlyTokenProvider _tokenProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="XShiftKeySource" /> class.
    /// </summary>
    /// <param name="httpClientFactory">The http client factory.</param>
    /// <param name="options">The options.</param>
    /// <param name="tokenProvider">The token provider.</param>
    /// <param name="logger">The logger.</param>
    public XShiftKeySource(IHttpClientFactory httpClientFactory, IOptionsMonitor<ShiftKeyScannerOptions> options, XAppOnlyTokenProvider tokenProvider, ILogger<XShiftKeySource> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _tokenProvider = tokenProvider;
        _logger = logger;
    }

    public string Name => "x";

    /// <summary>
    ///     Gets the keys asynchronously.
    /// </summary>
    /// <param name="sinceUtc">The since utc.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public async Task<IReadOnlyList<ShiftKeyCandidate>> GetKeysAsync(DateTimeOffset sinceUtc, CancellationToken cancellationToken = default)
    {
        var settings = _options.CurrentValue?.X;
        if (settings is null || !settings.Enabled)
        {
            return Array.Empty<ShiftKeyCandidate>();
        }

        if (settings.Usernames is null || (settings.Usernames.Count == 0))
        {
            return Array.Empty<ShiftKeyCandidate>();
        }

        var client = _httpClientFactory.CreateClient(HttpClientName);
        var results = new List<ShiftKeyCandidate>();

        var token = await _tokenProvider.GetBearerTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("X source is enabled but no bearer token could be generated.");

            return Array.Empty<ShiftKeyCandidate>();
        }

        var apiBaseV2 = string.IsNullOrWhiteSpace(settings.ApiBaseV2)
            ? "https://api.x.com/2/"
            : settings.ApiBaseV2;

        foreach (var username in settings.Usernames
                                         .Select(u => u?.Trim())
                                         .Where(u => !string.IsNullOrWhiteSpace(u)))
        {
            var userId = await GetUserIdAsync(client, apiBaseV2, token, username!, cancellationToken);
            if (string.IsNullOrWhiteSpace(userId))
            {
                continue;
            }

            var tweets = await GetUserTweetsV2Async(client, apiBaseV2, token, settings, userId, sinceUtc, cancellationToken);
            foreach (var tweet in tweets)
            {
                if (tweet.CreatedAt.HasValue && (tweet.CreatedAt.Value < sinceUtc))
                {
                    continue;
                }

                var text = tweet.Text ?? string.Empty;
                foreach (var code in ShiftKeyMatcher.Extract(text))
                {
                    results.Add(new ShiftKeyCandidate(code, $"x:{username}", tweet.CreatedAt));
                }
            }
        }

        return results;
    }

    /// <summary>
    ///     Builds the request.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <param name="bearerToken">The bearer token.</param>
    /// <returns>The result of the operation.</returns>
    private static HttpRequestMessage BuildRequest(string url, string bearerToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        return request;
    }

    /// <summary>
    ///     Builds the url.
    /// </summary>
    /// <param name="apiBase">The api base.</param>
    /// <param name="relativePath">The relative path.</param>
    /// <param name="query">The query.</param>
    /// <returns>The resulting string.</returns>
    private static string BuildUrl(string apiBase, string relativePath, Dictionary<string, string?> query)
    {
        var baseUri = apiBase?.TrimEnd('/');
        var path = relativePath.TrimStart('/');

        var url = string.IsNullOrWhiteSpace(baseUri)
            ? $"/{path}"
            : $"{baseUri}/{path}";

        var hasQuery = false;
        foreach (var kvp in query)
        {
            if (string.IsNullOrWhiteSpace(kvp.Value))
            {
                continue;
            }

            url += hasQuery
                ? "&"
                : "?";
            url += $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}";
            hasQuery = true;
        }

        return url;
    }

    /// <summary>
    ///     Formats the rfc 3339 utc.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The resulting string.</returns>
    private static string FormatRfc3339Utc(DateTimeOffset value)
    {
        return value.ToUniversalTime()
                    .ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Gets the user id asynchronously.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="apiBase">The api base.</param>
    /// <param name="bearerToken">The bearer token.</param>
    /// <param name="username">The username.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    private async Task<string?> GetUserIdAsync(HttpClient client, string apiBase, string bearerToken, string username, CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string?> { ["user.fields"] = "id" };

        var url = BuildUrl(apiBase, $"users/by/username/{Uri.EscapeDataString(username)}", query);

        using var response = await client.SendAsync(BuildRequest(url, bearerToken), cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await ReadBodyAsync(response, cancellationToken);
            _logger.LogWarning("X v2 user lookup failed for {Username}. Status: {StatusCode}. Body: {Body}", username, (int)response.StatusCode, body);

            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<XUserLookupResponse>(stream, SerializerOptions, cancellationToken);
        var id = payload?.Data?.Id;
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("X v2 user lookup returned no id for {Username}.", username);
        }

        return id;
    }

    /// <summary>
    ///     Gets the user tweets v 2 asynchronously.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="apiBase">The api base.</param>
    /// <param name="bearerToken">The bearer token.</param>
    /// <param name="settings">The settings.</param>
    /// <param name="userId">The user id.</param>
    /// <param name="sinceUtc">The since utc.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    private async Task<IReadOnlyList<XV2Tweet>> GetUserTweetsV2Async(HttpClient client, string apiBase, string bearerToken, ShiftKeyScannerOptions.XSourceOptions settings, string userId, DateTimeOffset sinceUtc, CancellationToken cancellationToken)
    {
        var tweets = new List<XV2Tweet>();
        var maxResults = Math.Clamp(settings.MaxResultsPerPage, 5, 100);
        var maxPages = Math.Clamp(settings.MaxPages, 1, 20);
        var startTime = FormatRfc3339Utc(sinceUtc);

        string? nextToken = null;

        for (var page = 0; page < maxPages; page++)
        {
            var query = new Dictionary<string, string?>
            {
                ["max_results"] = maxResults.ToString(CultureInfo.InvariantCulture),
                ["tweet.fields"] = "created_at",
                ["start_time"] = startTime,
                ["pagination_token"] = nextToken
            };

            var exclude = new List<string>();
            if (settings.ExcludeReplies)
            {
                exclude.Add("replies");
            }

            if (settings.ExcludeRetweets)
            {
                exclude.Add("retweets");
            }

            if (exclude.Count > 0)
            {
                query["exclude"] = string.Join(',', exclude);
            }

            var url = BuildUrl(apiBase, $"users/{userId}/tweets", query);

            using var response = await client.SendAsync(BuildRequest(url, bearerToken), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await ReadBodyAsync(response, cancellationToken);
                _logger.LogWarning("X v2 tweet fetch failed for user {UserId}. Status: {StatusCode}. Body: {Body}", userId, (int)response.StatusCode, body);

                break;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<XV2TweetResponse>(stream, SerializerOptions, cancellationToken);
            if (payload?.Data is null || (payload.Data.Count == 0))
            {
                break;
            }

            tweets.AddRange(payload.Data);

            nextToken = payload.Meta?.NextToken;
            if (string.IsNullOrWhiteSpace(nextToken))
            {
                break;
            }
        }

        return tweets;
    }

    /// <summary>
    ///     Reads the body asynchronously.
    /// </summary>
    /// <param name="response">The response.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    private static async Task<string?> ReadBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            const int maxLength = 2048;
            if (content.Length <= maxLength)
            {
                return content;
            }

            return content.Substring(0, maxLength) + "...(truncated)";
        }
        catch
        {
            return null;
        }
    }

    private sealed record XUserLookupResponse
    {
        [JsonPropertyName("data")]
        public XUserData? Data { get; init; }
    }

    private sealed record XUserData
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }
    }

    private sealed record XV2TweetResponse
    {
        [JsonPropertyName("data")]
        public List<XV2Tweet>? Data { get; init; }

        [JsonPropertyName("meta")]
        public XV2Meta? Meta { get; init; }
    }

    private sealed record XV2Meta
    {
        [JsonPropertyName("next_token")]
        public string? NextToken { get; init; }
    }

    private sealed record XV2Tweet
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("text")]
        public string? Text { get; init; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset? CreatedAt { get; init; }
    }
}
