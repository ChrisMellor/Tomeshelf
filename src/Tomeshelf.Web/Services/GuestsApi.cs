using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Models;
using Tomeshelf.Web.Models.ComicCon;

namespace Tomeshelf.Web.Services;

/// <summary>
/// HTTP client wrapper for calling the Tomeshelf API to get Comic Con guests.
/// </summary>
/// <param name="http">Configured <see cref="HttpClient"/> with API base address.</param>
/// <param name="logger">Logger for request/response telemetry.</param>
public sealed class GuestsApi(HttpClient http, ILogger<GuestsApi> logger) : IGuestsApi
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };
    private readonly ILogger<GuestsApi> _logger = logger;

    /// <summary>
    /// Calls the API to retrieve Comic Con guests for a city and returns the parsed groups with a total count.
    /// </summary>
    /// <param name="city">City name to query.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A tuple of groups and total guests.</returns>
    /// <exception cref="HttpRequestException">Thrown when the HTTP response is unsuccessful.</exception>
    /// <exception cref="JsonException">Thrown when the response body cannot be parsed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the response payload is empty.</exception>
    public async Task<(IReadOnlyList<GuestsGroupModel> Groups, int Total)> GetComicConGuestsByCityAsync(string city, CancellationToken cancellationToken)
    {
        var url = $"api/ComicCon/Guests/City?city={Uri.EscapeDataString(city)}";

        var started = DateTimeOffset.UtcNow;
        using var res = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var duration = DateTimeOffset.UtcNow - started;
        _logger.LogInformation("HTTP GET {Url} -> {Status} in {Duration}ms", url, (int)res.StatusCode, (int)duration.TotalMilliseconds);
        res.EnsureSuccessStatusCode();

        await using var s = await res.Content.ReadAsStreamAsync(cancellationToken);
        var env = await JsonSerializer.DeserializeAsync<GroupedEnvelope>(s, Json, cancellationToken)
                  ?? throw new InvalidOperationException("Empty payload");
        _logger.LogInformation("Parsed groups={Groups} total={Total}", env.Groups?.Count ?? 0, env.Total);

        return (env.Groups ?? [], env.Total);
    }

    private sealed class GroupedEnvelope
    {
        public string City { get; set; }
        public int Total { get; set; }
        public List<GuestsGroupModel> Groups { get; set; }
    }

    public async Task<GuestsByCityResult> GetComicConGuestsByCityResultAsync(string city, CancellationToken cancellationToken)
    {
        var (groups, total) = await GetComicConGuestsByCityAsync(city, cancellationToken);
        return new GuestsByCityResult(groups, total);
    }
}
