using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Tomeshelf.Application.Contracts;

namespace Tomeshelf.Infrastructure.Clients;

/// <summary>
/// HTTP client for accessing the external Comic Con People API.
/// </summary>
public class GuestsClient : IGuestsClient
{
    private readonly ILogger<IGuestsClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GuestsClient"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public GuestsClient(ILogger<IGuestsClient> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Calls the external Comic Con API to fetch the latest guests.
    /// </summary>
    /// <param name="key">The Comic Con API key for the target event/city.</param>
    /// <returns>The deserialized event payload; may be null if the body is empty.</returns>
    /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
    /// <exception cref="Exception">Thrown when the API returns a non-success status code.</exception>
    /// <exception cref="JsonException">Thrown when the response body cannot be parsed.</exception>
    public async Task<EventDto> GetLatestGuests(Guid key)
    {
        var url = $"https://conventions.leapevent.tech/api/people?key={key}";

        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to fetch guests: {response.ReasonPhrase}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var guests = JsonSerializer.Deserialize<EventDto>(content);

        _logger.LogInformation("Guests for Comic Con with key {key}: {content}", key, content);

        return guests;
    }
}
