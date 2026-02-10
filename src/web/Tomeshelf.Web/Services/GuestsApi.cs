using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Tomeshelf.Web.Models;
using Tomeshelf.Web.Models.ComicCon;
using Tomeshelf.Web.Models.Mcm;

namespace Tomeshelf.Web.Services;

/// <summary>
///     Provides methods for retrieving Comic Con guest and event information from the remote API, including support for
///     grouping guests and caching event data.
/// </summary>
/// <remarks>
///     This class is sealed and is intended for use as an API client for Comic Con guest and event data.
///     Event data is cached in memory to reduce redundant API calls. All methods are thread-safe when used with properly
///     configured dependencies.
/// </remarks>
public sealed class GuestsApi : IGuestsApi
{
    public const string HttpClientName = "Web.Guests";
    private const string EventsCacheKey = "comiccon.events";
    private static readonly TimeSpan EventsCacheDuration = TimeSpan.FromHours(3);

    private static readonly JsonSerializerOptions Json = new JsonSerializerOptions(JsonSerializerDefaults.Web) { NumberHandling = JsonNumberHandling.AllowReadingFromString };

    private readonly IMemoryCache _cache;
    private readonly HttpClient _http;

    private readonly ILogger<GuestsApi> _logger;

    /// <summary>
    ///     Initializes a new instance of the GuestsApi class with the specified HTTP client, logger, and memory cache.
    /// </summary>
    /// <param name="httpClientFactory">Factory used to resolve the named HTTP client for the guests API.</param>
    /// <param name="logger">The logger used to record diagnostic and operational information for the GuestsApi.</param>
    /// <param name="cache">The memory cache used to store and retrieve guest-related data for improved performance.</param>
    public GuestsApi(IHttpClientFactory httpClientFactory, ILogger<GuestsApi> logger, IMemoryCache cache)
    {
        _http = httpClientFactory.CreateClient(HttpClientName);
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    ///     Asynchronously retrieves the list of available Comic Con event configurations.
    /// </summary>
    /// <remarks>
    ///     The result is cached for subsequent calls to improve performance. Only events with both a valid ID
    ///     and name are included in the returned list. The list is ordered alphabetically by event name, using a
    ///     case-insensitive comparison.
    /// </remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    ///     A read-only list of event configuration models representing the available Comic Con events. The list is empty if no
    ///     events are found.
    /// </returns>
    public async Task<IReadOnlyList<McmEventConfigModel>> GetComicConEventsAsync(CancellationToken cancellationToken, bool forceRefresh = false)
    {
        if (!forceRefresh && _cache.TryGetValue(EventsCacheKey, out IReadOnlyList<McmEventConfigModel> cached))
        {
            return cached;
        }

        var url = "Config";
        var started = DateTimeOffset.UtcNow;
        using var res = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var duration = DateTimeOffset.UtcNow - started;
        _logger.LogInformation("HTTP GET {Url} -> {Status} in {Duration}ms", url, (int)res.StatusCode, (int)duration.TotalMilliseconds);

        res.EnsureSuccessStatusCode();

        await using var s = await res.Content.ReadAsStreamAsync(cancellationToken);
        var events = await JsonSerializer.DeserializeAsync<List<McmEventConfigModel>>(s, Json, cancellationToken) ?? new List<McmEventConfigModel>();

        var ordered = events.Where(e => !string.IsNullOrWhiteSpace(e.Id) && !string.IsNullOrWhiteSpace(e.Name))
                            .OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                            .ToList();

        _cache.Set(EventsCacheKey, ordered, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = EventsCacheDuration });

        return ordered;
    }

    /// <summary>
    ///     Inserts or updates the comic con event asynchronously.
    /// </summary>
    /// <param name="eventId">The event id.</param>
    /// <param name="name">The name.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task UpsertComicConEventAsync(string eventId, string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            throw new ArgumentException("Event ID is required.", nameof(eventId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Event name is required.", nameof(name));
        }

        var url = "Config";
        var started = DateTimeOffset.UtcNow;
        var payload = new { Id = eventId.Trim(), Name = name.Trim() };

        using var res = await _http.PutAsJsonAsync(url, payload, Json, cancellationToken);
        var duration = DateTimeOffset.UtcNow - started;
        _logger.LogInformation("HTTP PUT {Url} -> {Status} in {Duration}ms", url, (int)res.StatusCode, (int)duration.TotalMilliseconds);

        res.EnsureSuccessStatusCode();

        _cache.Remove(EventsCacheKey);
    }

    /// <summary>
    ///     Deletes the comic con event asynchronously.
    /// </summary>
    /// <param name="eventId">The event id.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public async Task<bool> DeleteComicConEventAsync(string eventId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            throw new ArgumentException("Event ID is required.", nameof(eventId));
        }

        var safeId = Uri.EscapeDataString(eventId.Trim());
        var url = $"Config/{safeId}";
        var started = DateTimeOffset.UtcNow;

        using var res = await _http.DeleteAsync(url, cancellationToken);
        var duration = DateTimeOffset.UtcNow - started;
        _logger.LogInformation("HTTP DELETE {Url} -> {Status} in {Duration}ms", url, (int)res.StatusCode, (int)duration.TotalMilliseconds);

        if (res.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        res.EnsureSuccessStatusCode();

        _cache.Remove(EventsCacheKey);

        return true;
    }

    /// <summary>
    ///     Asynchronously retrieves the list of guest groups and the total number of guests for a specified Comic Con event.
    /// </summary>
    /// <param name="eventId">
    ///     The unique identifier of the Comic Con event for which to retrieve guest information. Cannot be
    ///     null or empty.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a tuple with a read-only list of guest
    ///     groups and the total number of guests for the specified event.
    /// </returns>
    public async Task<(IReadOnlyList<GuestsGroupModel> Groups, int Total)> GetComicConGuestsByEventAsync(string eventId, CancellationToken cancellationToken)
    {
        var result = await GetComicConGuestsByEventResultAsync(eventId, cancellationToken);

        return (result.Groups, result.Total);
    }

    /// <summary>
    ///     Calls the API to retrieve Comic Con guests for an event and returns the parsed response model.
    /// </summary>
    /// <param name="eventId">Event identifier to query.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The deserialized API payload.</returns>
    /// <exception cref="HttpRequestException">Thrown when the HTTP response is unsuccessful.</exception>
    /// <exception cref="JsonException">Thrown when the response body cannot be parsed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the response payload is empty.</exception>
    public async Task<GuestsByEventResult> GetComicConGuestsByEventResultAsync(string eventId, CancellationToken cancellationToken)
    {
        const int pageSize = 200;
        var page = 1;
        var allGuests = new List<McmGuestDto>();

        while (true)
        {
            var pageResult = await GetGuestsPageAsync(eventId, page, pageSize, cancellationToken);

            if (pageResult.Items?.Count > 0)
            {
                allGuests.AddRange(pageResult.Items);
            }

            if (pageResult.Items is null || (pageResult.Items.Count == 0) || (allGuests.Count >= pageResult.Total))
            {
                break;
            }

            page++;
        }

        var total = allGuests.Count;
        var groups = BuildGroups(allGuests);

        return new GuestsByEventResult(groups, total);
    }

    /// <summary>
    ///     Groups the specified guests by the date they were added and returns a list of guest groups ordered by date
    ///     descending.
    /// </summary>
    /// <param name="guests">
    ///     The collection of guests to group. Each guest is grouped by the UTC date on which they were added.
    ///     Cannot be null.
    /// </param>
    /// <returns>
    ///     A list of guest group models, each containing guests added on the same date. The list is ordered by date
    ///     descending,
    ///     and guests within each group are ordered by last name and then by first name.
    /// </returns>
    private static List<GuestsGroupModel> BuildGroups(IEnumerable<McmGuestDto> guests)
    {
        return guests.GroupBy(g => g.AddedAt.UtcDateTime.Date)
                     .OrderByDescending(g => g.Key)
                     .Select(group => new GuestsGroupModel
                      {
                          CreatedDate = group.Key,
                          Items = group.Select(MapGuest)
                                       .OrderBy(p => p.LastName, StringComparer.OrdinalIgnoreCase)
                                       .ThenBy(p => p.FirstName, StringComparer.OrdinalIgnoreCase)
                                       .ToList()
                      })
                     .ToList();
    }

    /// <summary>
    ///     Asynchronously retrieves a paged list of guests for the specified event.
    /// </summary>
    /// <remarks>
    ///     Deleted guests are included in the results. The method throws an exception if the HTTP request is
    ///     unsuccessful or if the response cannot be deserialized.
    /// </remarks>
    /// <param name="eventId">The unique identifier of the event for which to retrieve guests. Cannot be null or empty.</param>
    /// <param name="page">The zero-based index of the page to retrieve. Must be greater than or equal to 0.</param>
    /// <param name="pageSize">The maximum number of guests to include in the page. Must be greater than 0.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a PagedEnvelope with the guests for the
    ///     specified page. The result is never null.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the response payload is empty or cannot be deserialized.</exception>
    private async Task<PagedEnvelope> GetGuestsPageAsync(string eventId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var url = $"events/{Uri.EscapeDataString(eventId)}/guests?page={page}&pageSize={pageSize}&includeDeleted=true";
        var started = DateTimeOffset.UtcNow;
        using var res = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var duration = DateTimeOffset.UtcNow - started;
        var safeUrl = url.Replace("\r", string.Empty)
                         .Replace("\n", string.Empty);
        _logger.LogInformation("HTTP GET {Url} -> {Status} in {Duration}ms", safeUrl, (int)res.StatusCode, (int)duration.TotalMilliseconds);

        res.EnsureSuccessStatusCode();

        await using var s = await res.Content.ReadAsStreamAsync(cancellationToken);
        var env = await JsonSerializer.DeserializeAsync<PagedEnvelope>(s, Json, cancellationToken) ?? throw new InvalidOperationException("Empty payload");

        return env;
    }

    /// <summary>
    ///     Maps a <see cref="McmGuestDto" /> instance to a <see cref="PersonModel" /> representation suitable for use in the
    ///     application.
    /// </summary>
    /// <remarks>
    ///     All string properties in the resulting <see cref="PersonModel" /> are guaranteed to be non-null. Image
    ///     URLs are trimmed and set to null if empty or whitespace. The <see cref="PersonModel.PubliclyVisible" /> property is
    ///     set to <see langword="true" /> if the guest is not marked as deleted.
    /// </remarks>
    /// <param name="guest">The guest data transfer object containing information to be mapped. Cannot be null.</param>
    /// <returns>A <see cref="PersonModel" /> populated with data from the specified <paramref name="guest" />.</returns>
    private static PersonModel MapGuest(McmGuestDto guest)
    {
        var (firstName, lastName) = SplitName(guest.Name);
        var imageUrl = string.IsNullOrWhiteSpace(guest.ImageUrl)
            ? null
            : guest.ImageUrl.Trim();
        var images = string.IsNullOrWhiteSpace(imageUrl)
            ? new List<ImageSetModel>()
            : new List<ImageSetModel>
            {
                new ImageSetModel
                {
                    Big = imageUrl,
                    Med = imageUrl,
                    Small = imageUrl,
                    Thumb = imageUrl
                }
            };

        return new PersonModel
        {
            Id = guest.Id.ToString(),
            Uid = string.Empty,
            PubliclyVisible = !guest.IsDeleted,
            FirstName = firstName,
            LastName = lastName,
            AltName = string.Empty,
            Bio = guest.Description ?? string.Empty,
            KnownFor = guest.Description ?? string.Empty,
            ProfileUrl = guest.ProfileUrl ?? string.Empty,
            ProfileUrlLabel = string.Empty,
            VideoLink = string.Empty,
            Twitter = string.Empty,
            Facebook = string.Empty,
            Instagram = string.Empty,
            YouTube = string.Empty,
            Twitch = string.Empty,
            Snapchat = string.Empty,
            DeviantArt = string.Empty,
            Tumblr = string.Empty,
            Category = string.Empty,
            DaysAtShow = string.Empty,
            BoothNumber = string.Empty,
            PeopleCategories = new List<object>(),
            Images = images,
            Schedules = new List<ScheduleModel>(),
            RemovedAt = guest.RemovedAt?.ToString("O") ?? string.Empty
        };
    }

    /// <summary>
    ///     Splits a full name string into first name and last name components.
    /// </summary>
    /// <remarks>
    ///     If the input contains multiple spaces, the last space is used to determine the split between first
    ///     and last name. Any whitespace around the resulting names is trimmed.
    /// </remarks>
    /// <param name="name">
    ///     The full name to split. Leading and trailing whitespace is ignored. If null, empty, or consists only of whitespace,
    ///     both components will be empty strings.
    /// </param>
    /// <returns>
    ///     A tuple containing the first name and last name. If the input does not contain a space, the entire trimmed input is
    ///     returned as the first name and the last name is an empty string. If the input is null, empty, or whitespace, both
    ///     values are empty strings.
    /// </returns>
    private static (string FirstName, string LastName) SplitName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return (string.Empty, string.Empty);
        }

        var trimmed = name.Trim();
        var lastSpace = trimmed.LastIndexOf(' ');

        if (lastSpace <= 0)
        {
            return (trimmed, string.Empty);
        }

        var firstName = trimmed[..lastSpace]
           .Trim();
        var lastName = trimmed[(lastSpace + 1)..]
           .Trim();

        return (firstName, lastName);
    }

    /// <summary>
    ///     Represents a paged result set containing a subset of items and pagination metadata.
    /// </summary>
    /// <remarks>
    ///     Use this type to encapsulate a single page of results when implementing pagination in APIs or data
    ///     queries. The properties provide information about the total number of items, the current page, the page size, and
    ///     the items included in the current page.
    /// </remarks>
    private sealed class PagedEnvelope
    {
        public int Total { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public List<McmGuestDto> Items { get; set; } = [];
    }
}
