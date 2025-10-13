using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tomeshelf.Application.Contracts;

namespace Tomeshelf.Infrastructure.Clients;

/// <summary>
///     HTTP client for accessing the external Comic Con People API.
/// </summary>
public class GuestsClient : IGuestsClient
{
    private readonly HttpClient _http;
    private readonly ILogger<GuestsClient> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GuestsClient" /> class.
    /// </summary>
    /// <param name="http">Injected HttpClient.</param>
    /// <param name="logger">Logger instance.</param>
    public GuestsClient(HttpClient http, ILogger<GuestsClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <summary>
    ///     Calls the external Comic Con API to fetch the latest guests.
    /// </summary>
    /// <param name="key">The Comic Con API key for the target event/city.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized event payload; may be null if the body is empty.</returns>
    /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
    /// <exception cref="JsonException">Thrown when the response body cannot be parsed.</exception>
    public async Task<EventDto> GetLatestGuestsAsync(Guid key, CancellationToken cancellationToken = default)
    {
        var url = $"https://conventions.leapevent.tech/api/people?key={key}";
        _logger.LogInformation("HTTP GET {Url}", url);

        using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var s = await response.Content.ReadAsStreamAsync(cancellationToken);
        var guests = await JsonSerializer.DeserializeAsync<EventDto>(s, cancellationToken: cancellationToken);
        _logger.LogInformation("Fetched guests payload ({Length} bytes)", response.Content.Headers.ContentLength?.ToString() ?? "?");

        return guests!;
    }
}