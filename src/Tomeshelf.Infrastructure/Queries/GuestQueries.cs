using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Contracts;
using Tomeshelf.Infrastructure.Persistence;

namespace Tomeshelf.Infrastructure.Queries;

/// <summary>
/// Read-only query operations for guests, categories and groupings.
/// </summary>
/// <param name="db">EF Core DbContext.</param>
/// <param name="logger">Logger instance.</param>
public sealed class GuestQueries(TomeshelfDbContext db, ILogger<GuestQueries> logger)
{
    private readonly TomeshelfDbContext _db = db;
    private readonly ILogger<GuestQueries> _logger = logger;

    /// <summary>
    /// Queries guests for a given event slug with optional day and text filters and paging.
    /// Returns items projected to DTOs and the total count before paging.
    /// </summary>
    /// <param name="eventSlug">Event slug to match.</param>
    /// <param name="day">Optional day filter (exact match within DaysAtShow).</param>
    /// <param name="search">Optional text to search in name or KnownFor.</param>
    /// <param name="page">1-based page index.</param>
    /// <param name="pageSize">Page size (1-100).</param>
    /// <param name="cancellationToken">Cancellation token for the query.</param>
    /// <returns>A tuple of items and total row count.</returns>
    /// <exception cref="OperationCanceledException">Thrown if the query is canceled.</exception>
    public async Task<(IReadOnlyList<PersonDto> Items, int Total)> GetGuestsAsync(string eventSlug, string day = null, string search = null, int page = 1, int pageSize = 24, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        _logger.LogInformation("Querying guests for event {EventSlug} (page={Page}, size={Size})", eventSlug, page, pageSize);
        var started = DateTimeOffset.UtcNow;

        var eventId = await _db.Events
            .Where(e => e.Slug == eventSlug)
            .Select(e => e.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (eventId == 0)
        {
            _logger.LogWarning("Event not found for slug {EventSlug}", eventSlug);
            return (Array.Empty<PersonDto>(), 0);
        }

        var query = _db.EventAppearances.AsNoTracking()
            .Where(eventAppearance => eventAppearance.EventId == eventId)
            .Select(eventAppearance => new
            {
                EventAppearance = eventAppearance,
                Person = eventAppearance.Person,
                Categories = eventAppearance.Person.Categories.Select(personCategory => personCategory.Category),
                Img = eventAppearance.Person.Images
                    .OrderByDescending(i => i.Id)
                    .Select(personImage => new ImageSetDto { Big = personImage.Big, Med = personImage.Med, Small = personImage.Small, Thumb = personImage.Thumb })
                    .FirstOrDefault(),
                Schedules = eventAppearance.Schedules.OrderBy(s => s.StartTimeUtc).Select(s => new ScheduleDto
                {
                    Id = s.ExternalId,
                    Title = s.Title,
                    Description = s.Description,
                    StartTime = s.StartTimeUtc.ToString("o"),
                    EndTime = s.EndTimeUtc.HasValue ? s.EndTimeUtc.Value.ToString("o") : null,
                    NoEndTime = s.NoEndTime,
                    Location = s.Location,
                    VenueLocation = s.VenueLocation == null ? null : new VenueLocationDto
                    {
                        Id = s.VenueLocation.ExternalId,
                        Name = s.VenueLocation.Name
                    }
                })
            });

        if (!string.IsNullOrWhiteSpace(day))
        {
            query = query.Where(x => x.EventAppearance.DaysAtShow != null && x.EventAppearance.DaysAtShow.Contains(day!));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(x =>
                (x.Person.FirstName + " " + x.Person.LastName).Contains(s) ||
                (x.Person.KnownFor ?? "").Contains(s));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Person.LastName).ThenBy(x => x.Person.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PersonDto
            {
                Id = x.Person.ExternalId,
                Uid = x.Person.Uid,
                PubliclyVisible = x.Person.PubliclyVisible,
                FirstName = x.Person.FirstName,
                LastName = x.Person.LastName,
                AltName = x.Person.AltName,
                Bio = x.Person.Bio,
                KnownFor = x.Person.KnownFor,
                ProfileUrl = x.Person.ProfileUrl,
                ProfileUrlLabel = x.Person.ProfileUrlLabel,
                VideoLink = x.Person.VideoLink,
                Twitter = x.Person.Twitter,
                Facebook = x.Person.Facebook,
                Instagram = x.Person.Instagram,
                YouTube = x.Person.YouTube,
                Twitch = x.Person.Twitch,
                Snapchat = x.Person.Snapchat,
                DeviantArt = x.Person.DeviantArt,
                Tumblr = x.Person.Tumblr,
                Category = null,
                DaysAtShow = x.EventAppearance.DaysAtShow,
                BoothNumber = x.EventAppearance.BoothNumber,
                AutographAmount = x.EventAppearance.AutographAmount,
                PhotoOpAmount = x.EventAppearance.PhotoOpAmount,
                PhotoOpTableAmount = x.EventAppearance.PhotoOpTableAmount,
                PeopleCategories = null,
                GlobalCategories = x.Categories
                    .Select(c => new CategoryDto { Id = c.ExternalId, Name = c.Name, Color = null })
                    .ToList(),
                Images = x.Img == null ? new List<ImageSetDto>() : new List<ImageSetDto> { x.Img },
                Schedules = x.Schedules.ToList()
            })
            .ToListAsync(cancellationToken);

        var duration = DateTimeOffset.UtcNow - started;
        _logger.LogInformation("Guests query for {EventSlug} returned {Count} items (total={Total}) in {Duration}ms", eventSlug, items.Count, total, (int)duration.TotalMilliseconds);

        return (items, total);
    }

    /// <summary>
    /// Returns distinct categories linked to people for a given event slug.
    /// </summary>
    /// <param name="slug">Event slug.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of category id-name pairs.</returns>
    /// <exception cref="OperationCanceledException">Thrown if the query is canceled.</exception>
    public async Task<IReadOnlyList<(string Id, string Name)>> GetCategoriesByEventSlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Querying categories for event {EventSlug}", slug);
        var eventId = await _db.Events.Where(e => e.Slug == slug)
            .Select(e => e.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (eventId == 0)
        {
            _logger.LogWarning("Event not found for slug {EventSlug}", slug);

            return Array.Empty<(string, string)>();
        }

        var cats = await _db.EventAppearances
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

    public sealed record GuestsGroupResult(DateTime CreatedDate, IReadOnlyList<PersonDto> Items);

    /// <summary>
    /// Returns guests grouped by creation date for events whose slug contains the given city.
    /// </summary>
    /// <param name="city">City name fragment to search in the event slug.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of date-based groups with guest items.</returns>
    /// <exception cref="OperationCanceledException">Thrown if the query is canceled.</exception>
    public async Task<IReadOnlyList<GuestsGroupResult>> GetGuestsByCityAsync(string city, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            _logger.LogWarning("GuestsByCity called without a city parameter");

            return Array.Empty<GuestsGroupResult>();
        }

        var like = $"%{city}%";
        _logger.LogInformation("Querying guests by city like pattern {Pattern}", like);

        var baseQuery = _db.EventAppearances.AsNoTracking()
            .Where(a => EF.Functions.Like(a.Event.Slug, like))
            .Select(a => new
            {
                PersonCreated = a.Person.CreatedUtc,
                EventAppearance = a,
                Person = a.Person,
                Categories = a.Person.Categories.Select(pc => pc.Category),
                Img = a.Person.Images
                    .OrderByDescending(i => i.Id)
                    .Select(personImage => new ImageSetDto { Big = personImage.Big, Med = personImage.Med, Small = personImage.Small, Thumb = personImage.Thumb })
                    .FirstOrDefault(),
                Schedules = a.Schedules.OrderBy(s => s.StartTimeUtc).Select(s => new ScheduleDto
                {
                    Id = s.ExternalId,
                    Title = s.Title,
                    Description = s.Description,
                    StartTime = s.StartTimeUtc.ToString("o"),
                    EndTime = s.EndTimeUtc.HasValue ? s.EndTimeUtc.Value.ToString("o") : null,
                    NoEndTime = s.NoEndTime,
                    Location = s.Location,
                    VenueLocation = s.VenueLocation == null ? null : new VenueLocationDto
                    {
                        Id = s.VenueLocation.ExternalId,
                        Name = s.VenueLocation.Name
                    }
                })
            });

        var qStart = DateTimeOffset.UtcNow;
        var rows = await baseQuery
            .OrderByDescending(x => x.Person.CreatedUtc)
            .Select(x => new
            {
                Date = x.PersonCreated.Date,
                Person = new PersonDto
                {
                    Id = x.Person.ExternalId,
                    Uid = x.Person.Uid,
                    PubliclyVisible = x.Person.PubliclyVisible,
                    FirstName = x.Person.FirstName,
                    LastName = x.Person.LastName,
                    AltName = x.Person.AltName,
                    Bio = x.Person.Bio,
                    KnownFor = x.Person.KnownFor,
                    ProfileUrl = x.Person.ProfileUrl,
                    ProfileUrlLabel = x.Person.ProfileUrlLabel,
                    VideoLink = x.Person.VideoLink,
                    Twitter = x.Person.Twitter,
                    Facebook = x.Person.Facebook,
                    Instagram = x.Person.Instagram,
                    YouTube = x.Person.YouTube,
                    Twitch = x.Person.Twitch,
                    Snapchat = x.Person.Snapchat,
                    DeviantArt = x.Person.DeviantArt,
                    Tumblr = x.Person.Tumblr,
                    Category = null,
                    DaysAtShow = x.EventAppearance.DaysAtShow,
                    BoothNumber = x.EventAppearance.BoothNumber,
                    AutographAmount = x.EventAppearance.AutographAmount,
                    PhotoOpAmount = x.EventAppearance.PhotoOpAmount,
                    PhotoOpTableAmount = x.EventAppearance.PhotoOpTableAmount,
                    PeopleCategories = null,
                    GlobalCategories = x.Categories
                        .Select(c => new CategoryDto { Id = c.ExternalId, Name = c.Name, Color = null })
                        .ToList(),
                    Images = x.Img == null ? new List<ImageSetDto>() : new List<ImageSetDto> { x.Img },
                    Schedules = x.Schedules.ToList()
                }
            })
            .ToListAsync(cancellationToken);

        var groups = rows
            .GroupBy(x => x.Date)
            .OrderByDescending(g => g.Key)
            .Select(g => new GuestsGroupResult(g.Key, g.Select(r => r.Person).ToList()))
            .ToList();

        var took = DateTimeOffset.UtcNow - qStart;
        var total = groups.Sum(g => g.Items.Count);
        _logger.LogInformation("GuestsByCity for {City} returned {Groups} day-groups, {Total} guests in {Duration}ms", city, groups.Count, total, (int)took.TotalMilliseconds);

        return groups;
    }
}
