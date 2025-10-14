using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tomeshelf.Application.Contracts;
using Tomeshelf.Application.Options;
using Tomeshelf.Infrastructure.Clients;

namespace Tomeshelf.Infrastructure.Services;

/// <summary>
///     Domain service orchestrating retrieval and persistence of Comic Con guests.
/// </summary>
public class GuestService : IGuestService
{
    private readonly IReadOnlyDictionary<string, Guid> _cityKeyMap;
    private readonly IGuestsClient _guestsClient;
    private readonly EventIngestService _ingest;
    private readonly ILogger<GuestService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GuestService" /> class.
    /// </summary>
    /// <param name="guestsClient">External API client for guests.</param>
    /// <param name="options">Options containing Comic Con mappings.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="ingest">Service that persists event data.</param>
    public GuestService(IGuestsClient guestsClient, IOptions<ComicConOptions> options, ILogger<GuestService> logger, EventIngestService ingest)
    {
        _guestsClient = guestsClient;
        _logger = logger;
        _ingest = ingest;
        _cityKeyMap = options.Value.ComicCon.GroupBy(location => location.City, StringComparer.OrdinalIgnoreCase)
                             .ToDictionary(grouping => grouping.Key, grouping => grouping.First()
                                                                                         .Key, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Retrieves the latest guests for the configured Comic Con in the given city
    ///     using the external client, persists the data, and returns the people list.
    /// </summary>
    /// <param name="city">City name matching configuration.</param>
    /// <param name="cancellationToken">Token used to cancel the downstream HTTP call and ingest.</param>
    /// <returns>The list of people returned by the external API (possibly empty).</returns>
    /// <exception cref="ApplicationException">Thrown when the city is not configured or no guests are returned.</exception>
    /// <exception cref="HttpRequestException">Thrown when the external request fails.</exception>
    public async Task<List<PersonDto>> GetGuestsAsync(string city, CancellationToken cancellationToken = default)
    {
        var comicConKey = GetKeyFromConfig(city);

        var evt = await _guestsClient.GetLatestGuestsAsync(comicConKey, cancellationToken);
        if (evt is null)
        {
            _logger.LogWarning("No guests found for Comic Con with key {ComicConKey}", comicConKey);

            throw new ApplicationException($"No guests found for Comic Con key: '{comicConKey}'.");
        }

        var changed = await _ingest.UpsertAsync(evt, cancellationToken);
        _logger.LogInformation("Upserted event {Slug} with {Count} people; changed={Changed}", evt.EventSlug, evt.People?.Count ?? 0, changed);

        return evt.People ?? [];
    }

    /// <summary>
    ///     Resolves the Comic Con key (GUID) for the specified city from configuration.
    /// </summary>
    /// <param name="city">City name to resolve.</param>
    /// <returns>The configured Comic Con key.</returns>
    /// <exception cref="ApplicationException">Thrown when the city is not present in configuration.</exception>
    private Guid GetKeyFromConfig(string city)
    {
        if (!_cityKeyMap.TryGetValue(city, out var comicConKey))
        {
            _logger.LogError("No Comic Con configured for city {City}", city);

            throw new ApplicationException($"No Comic Con configured for city: '{city}'.");
        }

        _logger.LogInformation("Fetching latest guests for Comic Con in city {City}", city);

        return comicConKey;
    }
}