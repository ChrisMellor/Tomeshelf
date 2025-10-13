using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tomeshelf.Application.Contracts;
using Tomeshelf.Domain.Entities.ComicCon;
using Tomeshelf.Infrastructure.Persistence;

namespace Tomeshelf.Infrastructure.Queries;

/// <summary>
///     Read-only query operations for guests, categories and groupings.
/// </summary>
public sealed class GuestQueries
{
    private readonly TomeshelfComicConDbContext _comicConDb;
    private readonly ILogger<GuestQueries> _logger;

    /// <summary>
    ///     Constructor with dependencies injected.
    /// </summary>
    /// <param name="comicConDb">EF Core DbContext.</param>
    /// <param name="logger">Logger instance.</param>
    public GuestQueries(TomeshelfComicConDbContext comicConDb, ILogger<GuestQueries> logger)
    {
        _comicConDb = comicConDb;
        _logger = logger;
    }

    /// <summary>
    ///     Queries guests for a given event slug with optional day and text filters and paging.
    ///     Returns items projected to DTOs and the total count before paging.
    /// </summary>
    /// <param name="eventSlug">Event slug to match.</param>
    /// <param name="day">Optional day filter (exact match within DaysAtShow).</param>
    /// <param name="search">Optional text to search in name or KnownFor.</param>
    /// <param name="page">1-based page index.</param>
    /// <param name="pageSize">Page size (1-100).</param>
    /// <param name="cancellationToken">Cancellation token for the query.</param>
    /// <returns>A tuple of items and total row count.</returns>
    /// <exception cref="OperationCanceledException">Thrown if the query is canceled.</exception>
    public async Task<(IReadOnlyList<PersonDto> Items, int Total)> GetGuestsAsync(string eventSlug, string day = null,
        string search = null, int page = 1, int pageSize = 24, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        _logger.LogInformation("Querying guests for event {EventSlug} (page={Page}, size={Size})", eventSlug, page,
            pageSize);
        var started = DateTimeOffset.UtcNow;

        var eventId = await _comicConDb.Events
            .Where(e => e.Slug == eventSlug)
            .Select(e => e.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (eventId == 0)
        {
            _logger.LogWarning("Event not found for slug {EventSlug}", eventSlug);
            return (Array.Empty<PersonDto>(), 0);
        }

        var baseQuery = _comicConDb.EventAppearances.AsNoTracking()
            .Where(eventAppearance => eventAppearance.EventId == eventId);

        if (!string.IsNullOrWhiteSpace(day))
            baseQuery = baseQuery.Where(a => a.DaysAtShow != null && a.DaysAtShow.Contains(day!));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            baseQuery = baseQuery.Where(a =>
                (a.Person.FirstName + " " + a.Person.LastName).Contains(s) ||
                (a.Person.KnownFor ?? "").Contains(s));
        }

        var total = await baseQuery.CountAsync(cancellationToken);

        var ordered = baseQuery
            .OrderBy(a => a.Person.LastName)
            .ThenBy(a => a.Person.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        var items = await ProjectPeople(ordered)
            .Select(x => x.Person)
            .ToListAsync(cancellationToken);

        var duration = DateTimeOffset.UtcNow - started;
        _logger.LogInformation("Guests query for {EventSlug} returned {Count} items (total={Total}) in {Duration}ms",
            eventSlug, items.Count, total, (int)duration.TotalMilliseconds);

        return (items, total);
    }

    /// <summary>
    ///     Returns distinct categories linked to people for a given event slug.
    /// </summary>
    /// <param name="slug">Event slug.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of category id-name pairs.</returns>
    /// <exception cref="OperationCanceledException">Thrown if the query is canceled.</exception>
    public async Task<IReadOnlyList<(string Id, string Name)>> GetCategoriesByEventSlugAsync(string slug,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Querying categories for event {EventSlug}", slug);
        var eventId = await _comicConDb.Events.Where(e => e.Slug == slug)
            .Select(e => e.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (eventId == 0)
        {
            _logger.LogWarning("Event not found for slug {EventSlug}", slug);

            return Array.Empty<(string, string)>();
        }

        var cats = await _comicConDb.EventAppearances
            .Where(a => a.EventId == eventId)
            .SelectMany(a => a.Person.Categories.Select(pc => pc.Category))
            .Distinct()
            .OrderBy(c => c.Name)
            .Select(c => new { c.ExternalId, c.Name })
            .ToListAsync(cancellationToken);

        var list = cats.Select(c => (c.ExternalId, c.Name)).ToList();
        _logger.LogInformation("Categories query for {EventSlug} returned {Count} items", slug, list.Count);

        return list;
    }

    /// <summary>
    ///     Returns guests grouped by creation date for events whose slug contains the given city.
    /// </summary>
    /// <param name="city">City name fragment to search in the event slug.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of date-based groups with guest items.</returns>
    /// <exception cref="OperationCanceledException">Thrown if the query is canceled.</exception>
    public async Task<IReadOnlyList<GuestsGroupResult>> GetGuestsByCityAsync(string city,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            _logger.LogWarning("GuestsByCity called without a city parameter");

            return Array.Empty<GuestsGroupResult>();
        }

        var like = $"%{city}%";
        _logger.LogInformation("Querying guests by city like pattern {Pattern}", like);

        var baseQuery = _comicConDb.EventAppearances.AsNoTracking()
            .Where(a => EF.Functions.Like(a.Event.Slug, like));

        var qStart = DateTimeOffset.UtcNow;
        var rows = await ProjectPeople(baseQuery)
            .OrderByDescending(x => x.CreatedUtc)
            .ToListAsync(cancellationToken);

        var groups = rows
            .GroupBy(x => x.CreatedUtc.Date)
            .OrderByDescending(g => g.Key)
            .Select(g => new GuestsGroupResult(g.Key, g.Select(r => r.Person).ToList()))
            .ToList();

        var took = DateTimeOffset.UtcNow - qStart;
        var total = groups.Sum(g => g.Items.Count);
        _logger.LogInformation("GuestsByCity for {City} returned {Groups} day-groups, {Total} guests in {Duration}ms",
            city, groups.Count, total, (int)took.TotalMilliseconds);

        return groups;
    }

    private static IQueryable<PersonProjection> ProjectPeople(IQueryable<EventAppearance> source)
    {
        return source.Select(a => new PersonProjection
        {
            CreatedUtc = a.Person.CreatedUtc,
            Person = new PersonDto
            {
                Id = a.Person.ExternalId,
                Uid = a.Person.Uid,
                PubliclyVisible = a.Person.PubliclyVisible,
                FirstName = a.Person.FirstName,
                LastName = a.Person.LastName,
                AltName = a.Person.AltName,
                Bio = a.Person.Bio,
                KnownFor = a.Person.KnownFor,
                ProfileUrl = a.Person.ProfileUrl,
                ProfileUrlLabel = a.Person.ProfileUrlLabel,
                VideoLink = a.Person.VideoLink,
                Twitter = a.Person.Twitter,
                Facebook = a.Person.Facebook,
                Instagram = a.Person.Instagram,
                YouTube = a.Person.YouTube,
                Twitch = a.Person.Twitch,
                Snapchat = a.Person.Snapchat,
                DeviantArt = a.Person.DeviantArt,
                Tumblr = a.Person.Tumblr,
                RemovedAt = a.Person.RemovedUtc.HasValue ? a.Person.RemovedUtc.Value.ToString("o") : null,
                Category = null,
                DaysAtShow = a.DaysAtShow,
                BoothNumber = a.BoothNumber,
                AutographAmount = a.AutographAmount,
                PhotoOpAmount = a.PhotoOpAmount,
                PhotoOpTableAmount = a.PhotoOpTableAmount,
                PeopleCategories = null,
                GlobalCategories = a.Person.Categories
                    .Select(pc => pc.Category)
                    .Select(c => new CategoryDto { Id = c.ExternalId, Name = c.Name, Color = null })
                    .ToList(),
                Images = a.Person.Images
                    .OrderByDescending(i => i.Id)
                    .Select(personImage => new ImageSetDto
                    {
                        Big = personImage.Big, Med = personImage.Med, Small = personImage.Small,
                        Thumb = personImage.Thumb
                    })
                    .Take(1)
                    .ToList(),
                Schedules = a.Schedules
                    .OrderBy(s => s.StartTimeUtc)
                    .Select(s => new ScheduleDto
                    {
                        Id = s.ExternalId,
                        Title = s.Title,
                        Description = s.Description,
                        StartTime = s.StartTimeUtc.ToString("o"),
                        EndTime = s.EndTimeUtc.HasValue ? s.EndTimeUtc.Value.ToString("o") : null,
                        NoEndTime = s.NoEndTime,
                        Location = s.Location,
                        VenueLocation = s.VenueLocation == null
                            ? null
                            : new VenueLocationDto
                            {
                                Id = s.VenueLocation.ExternalId,
                                Name = s.VenueLocation.Name
                            }
                    })
                    .ToList()
            }
        });
    }

    public sealed record GuestsGroupResult(DateTime CreatedDate, IReadOnlyList<PersonDto> Items);

    private sealed class PersonProjection
    {
        public DateTime CreatedUtc { get; init; }

        public PersonDto Person { get; init; } = default!;
    }
}