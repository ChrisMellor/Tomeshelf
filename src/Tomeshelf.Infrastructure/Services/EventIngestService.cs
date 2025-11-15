using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tomeshelf.Application.Contracts;
using Tomeshelf.Domain.Entities.ComicCon;
using Tomeshelf.Infrastructure.Persistence;

namespace Tomeshelf.Infrastructure.Services;

/// <summary>
///     Handles ingesting event payloads into the database by upserting events, people, appearances, categories, images,
///     schedules, and venue locations.
/// </summary>
/// <param name="context">EF Core database context.</param>
public class EventIngestService(TomeshelfMcmDbContext context)
{
    /// <summary>
    ///     Inserts or updates the event and all related people, categories, images and schedules.
    ///     Creates new entities when missing and updates existing ones, then saves the changes.
    /// </summary>
    /// <param name="eventData">The source event payload to persist.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    /// <exception cref="FormatException">Thrown when schedule date/time strings are invalid.</exception>
    /// <exception cref="DbUpdateException">Thrown when database update fails.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public async Task<int> UpsertAsync(EventDto eventData, CancellationToken cancellationToken = default)
    {
        var entity = await UpsertEventAsync(eventData, cancellationToken);

        var categoryCache = await BuildCategoryCacheAsync(eventData, cancellationToken);

        foreach (var personData in eventData.People)
        {
            var person = await GetOrCreatePersonAsync(personData.Id, cancellationToken);
            UpdatePersonProperties(person, personData);
            SyncPersonImages(person, personData.Images);
            EnsureCategories(personData.GlobalCategories, categoryCache);
            SyncPersonCategories(person, personData.GlobalCategories, categoryCache);

            var appearance = await GetOrCreateAppearanceAsync(entity, person, cancellationToken);
            UpdateAppearanceProperties(appearance, personData);
            await SyncSchedulesAsync(appearance, personData.Schedules, cancellationToken);
        }

        return await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///     Retrieves an existing event by external id or creates a new tracked instance and updates its core properties.
    /// </summary>
    /// <param name="dto">Incoming event payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The tracked <see cref="Event" /> entity.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    private async Task<Event> UpsertEventAsync(EventDto dto, CancellationToken ct)
    {
        var entity = await context.Events.SingleOrDefaultAsync(x => x.ExternalId == dto.EventId, ct);
        if (entity is null)
        {
            entity = new Event
            {
                    ExternalId = dto.EventId,
                    Name = dto.EventName,
                    Slug = dto.EventSlug
            };
            context.Events.Add(entity);
        }
        else
        {
            entity.Name = dto.EventName;
            entity.Slug = dto.EventSlug;
            entity.UpdatedUtc = DateTime.UtcNow;
        }

        return entity;
    }

    /// <summary>
    ///     Builds a cache of categories referenced in the payload, reusing previously tracked entities when available.
    /// </summary>
    /// <param name="dto">Incoming event payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A dictionary keyed by external category id.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    private async Task<Dictionary<string, Category>> BuildCategoryCacheAsync(EventDto dto, CancellationToken ct)
    {
        var allCatIds = dto.People.SelectMany(p => p.GlobalCategories ?? [])
                           .Select(c => c.Id)
                           .Distinct()
                           .ToList();

        var cache = await context.Categories.Where(c => allCatIds.Contains(c.ExternalId))
                                 .ToDictionaryAsync(c => c.ExternalId, c => c, ct);

        return cache;
    }

    /// <summary>
    ///     Retrieves an existing person (including images and categories) or creates a new tracked instance.
    /// </summary>
    /// <param name="externalId">External person identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The tracked <see cref="Person" /> entity.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    private async Task<Person> GetOrCreatePersonAsync(string externalId, CancellationToken ct)
    {
        var person = await context.People.Include(x => x.Images)
                                  .Include(x => x.Categories)
                                  .ThenInclude(pc => pc.Category)
                                  .SingleOrDefaultAsync(x => x.ExternalId == externalId, ct);

        if (person is null)
        {
            person = new Person { ExternalId = externalId };
            context.People.Add(person);
        }

        return person;
    }

    /// <summary>
    ///     Updates mutable person fields and derives visibility/removal metadata from the incoming DTO.
    /// </summary>
    /// <param name="person">Tracked person entity to update.</param>
    /// <param name="data">Incoming person payload.</param>
    private static void UpdatePersonProperties(Person person, PersonDto data)
    {
        person.Uid = data.Uid;
        var wasVisible = person.PubliclyVisible;

        var isCanceledCategory = (data.GlobalCategories ?? []).Any(c => !string.IsNullOrWhiteSpace(c.Name) && (string.Equals(c.Name.Trim(), "Canceled", StringComparison.OrdinalIgnoreCase) || string.Equals(c.Name.Trim(), "Cancelled", StringComparison.OrdinalIgnoreCase)));

        var desiredVisible = data.PubliclyVisible && !isCanceledCategory;
        person.PubliclyVisible = desiredVisible;

        if (!desiredVisible && person.RemovedUtc is null)
        {
            if (wasVisible || isCanceledCategory)
            {
                person.RemovedUtc = DateTime.UtcNow;
            }
        }

        if (!wasVisible && desiredVisible)
        {
            person.RemovedUtc = null;
        }

        person.FirstName = data.FirstName ?? "";
        person.LastName = data.LastName ?? "";
        person.AltName = data.AltName;
        person.Bio = data.Bio;
        person.KnownFor = data.KnownFor;
        person.ProfileUrl = data.ProfileUrl;
        person.ProfileUrlLabel = data.ProfileUrlLabel;
        person.VideoLink = data.VideoLink;
        person.Twitter = data.Twitter;
        person.Facebook = data.Facebook;
        person.Instagram = data.Instagram;
        person.YouTube = data.YouTube;
        person.Twitch = data.Twitch;
        person.Snapchat = data.Snapchat;
        person.DeviantArt = data.DeviantArt;
        person.Tumblr = data.Tumblr;
        person.UpdatedUtc ??= DateTime.UtcNow;
    }

    /// <summary>
    ///     Synchronises the person's images collection to match the incoming payload while preserving existing entities when
    ///     possible.
    /// </summary>
    /// <param name="person">Tracked person entity.</param>
    /// <param name="images">Incoming image descriptors.</param>
    private static void SyncPersonImages(Person person, List<ImageSetDto> images)
    {
        var desired = (images ?? []).Select(img => (Key: BuildImageKey(img), Payload: img))
                                    .ToList();

        var existingGroups = person.Images.GroupBy(BuildImageKey, StringComparer.OrdinalIgnoreCase)
                                   .ToDictionary(group => group.Key, group => new Queue<PersonImage>(group), StringComparer.OrdinalIgnoreCase);

        foreach (var entry in desired)
        {
            if (existingGroups.TryGetValue(entry.Key, out var queue) && (queue.Count > 0))
            {
                var image = queue.Dequeue();
                image.Big = entry.Payload.Big;
                image.Med = entry.Payload.Med;
                image.Small = entry.Payload.Small;
                image.Thumb = entry.Payload.Thumb;
            }
            else
            {
                person.Images.Add(new PersonImage
                {
                        Big = entry.Payload.Big,
                        Med = entry.Payload.Med,
                        Small = entry.Payload.Small,
                        Thumb = entry.Payload.Thumb
                });
            }
        }

        var removals = existingGroups.Values.SelectMany(q => q)
                                     .ToList();
        foreach (var leftover in removals)
        {
            person.Images.Remove(leftover);
        }
    }

    /// <summary>
    ///     Ensures categories referenced in the payload exist in the shared cache, creating new entities when required.
    /// </summary>
    /// <param name="categories">Incoming category descriptors.</param>
    /// <param name="cache">Cache used to reuse tracked entities.</param>
    private void EnsureCategories(List<CategoryDto> categories, Dictionary<string, Category> cache)
    {
        foreach (var c in categories ?? [])
        {
            GetOrAddCategory(c.Id, c.Name, cache, context);
        }
    }

    /// <summary>
    ///     Aligns the person's category links with the payload, removing stale links and adding missing ones.
    /// </summary>
    /// <param name="person">Tracked person entity.</param>
    /// <param name="categories">Incoming category descriptors.</param>
    /// <param name="cache">Category cache for resolving entities.</param>
    private static void SyncPersonCategories(Person person, List<CategoryDto> categories, Dictionary<string, Category> cache)
    {
        var desired = (categories ?? []).Select(c => c.Id)
                                        .ToHashSet();

        person.Categories.RemoveWhere(link => !desired.Contains(link.Category.ExternalId));

        var existing = person.Categories.Select(x => x.Category.ExternalId)
                             .ToHashSet();

        foreach (var id in desired.Except(existing))
        {
            var category = cache[id];
            person.Categories.Add(new PersonCategory
            {
                    Person = person,
                    Category = category
            });
        }
    }

    /// <summary>
    ///     Retrieves or creates an <see cref="EventAppearance" /> for the given event/person pair.
    /// </summary>
    /// <param name="eventEntity">Tracked event entity.</param>
    /// <param name="person">Tracked person entity.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The tracked <see cref="EventAppearance" />.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    private async Task<EventAppearance> GetOrCreateAppearanceAsync(Event eventEntity, Person person, CancellationToken ct)
    {
        EventAppearance appearance = null;

        if ((eventEntity.Id > 0) && (person.Id > 0))
        {
            appearance = await context.EventAppearances.Include(x => x.Schedules)
                                      .SingleOrDefaultAsync(x => (x.EventId == eventEntity.Id) && (x.PersonId == person.Id), ct);
        }

        if (appearance is null)
        {
            appearance = new EventAppearance
            {
                    Event = eventEntity,
                    Person = person
            };
            context.EventAppearances.Add(appearance);
        }

        return appearance;
    }

    /// <summary>
    ///     Updates mutable appearance fields from the incoming payload.
    /// </summary>
    /// <param name="a">Tracked appearance entity.</param>
    /// <param name="data">Incoming person payload.</param>
    private static void UpdateAppearanceProperties(EventAppearance a, PersonDto data)
    {
        a.DaysAtShow = data.DaysAtShow;
        a.BoothNumber = data.BoothNumber;
        a.AutographAmount = data.AutographAmount;
        a.PhotoOpAmount = data.PhotoOpAmount;
        a.PhotoOpTableAmount = data.PhotoOpTableAmount;
    }

    /// <summary>
    ///     Synchronises child schedule entities for the provided appearance.
    /// </summary>
    /// <param name="a">Tracked appearance entity.</param>
    /// <param name="schedules">Incoming schedules payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="FormatException">Thrown when the schedule payload contains invalid timestamps.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    private async Task SyncSchedulesAsync(EventAppearance a, List<ScheduleDto> schedules, CancellationToken ct)
    {
        var byId = a.Schedules.ToDictionary(s => s.ExternalId);
        foreach (var sd in schedules ?? [])
        {
            if (!byId.TryGetValue(sd.Id, out var s))
            {
                s = new Schedule
                {
                        EventAppearance = a,
                        ExternalId = sd.Id
                };
                a.Schedules.Add(s);
            }

            s.Title = sd.Title;
            s.Description = sd.Description;
            s.StartTimeUtc = ParseAsUtc(sd.StartTime);
            s.EndTimeUtc = string.IsNullOrWhiteSpace(sd.EndTime)
                    ? null
                    : ParseAsUtc(sd.EndTime);
            s.NoEndTime = sd.NoEndTime;
            s.Location = sd.Location;

            if (sd.VenueLocation is not null)
            {
                var vl = await context.VenueLocations.SingleOrDefaultAsync(x => x.ExternalId == sd.VenueLocation.Id, ct);
                if (vl is null)
                {
                    vl = new VenueLocation
                    {
                            ExternalId = sd.VenueLocation.Id,
                            Name = sd.VenueLocation.Name
                    };
                    context.VenueLocations.Add(vl);
                }
                else
                {
                    vl.Name = sd.VenueLocation.Name;
                }

                s.VenueLocation = vl;
            }
            else
            {
                s.VenueLocation = null;
            }
        }
    }

    /// <summary>
    ///     Gets an existing category from the cache or creates and tracks a new one.
    ///     Ensures the category name is updated to the latest value.
    /// </summary>
    /// <param name="id">External category identifier.</param>
    /// <param name="name">Current category display name.</param>
    /// <param name="categoryCache">Cache of categories indexed by external id.</param>
    /// <param name="mcmDbContext">EF Core mcmDbContext used to add new entities.</param>
    /// <returns>The resolved <see cref="Category" />.</returns>
    private Category GetOrAddCategory(string id, string name, Dictionary<string, Category> categoryCache, TomeshelfMcmDbContext mcmDbContext)
    {
        if (categoryCache.TryGetValue(id, out var existing))
        {
            existing.Name = name;

            return existing;
        }

        var category = new Category
        {
                ExternalId = id,
                Name = name
        };
        mcmDbContext.Categories.Add(category);
        categoryCache[id] = category;

        return category;
    }

    /// <summary>
    ///     Parses a date-time string and returns the UTC DateTime.
    /// </summary>
    /// <param name="value">The date-time string representation.</param>
    /// <returns>A UTC <see cref="DateTime" />.</returns>
    private static DateTime ParseAsUtc(string value)
    {
        var dto = DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

        return dto.UtcDateTime;
    }

    /// <summary>
    ///     Builds a key used to compare incoming image payloads.
    /// </summary>
    /// <param name="image">Incoming image payload.</param>
    /// <returns>A deterministic key representing the image URLs.</returns>
    private static string BuildImageKey(ImageSetDto image)
    {
        return BuildImageKey(image.Big, image.Med, image.Small, image.Thumb);
    }

    /// <summary>
    ///     Builds a key used to compare tracked image entities.
    /// </summary>
    /// <param name="image">Tracked image entity.</param>
    /// <returns>A deterministic key representing the image URLs.</returns>
    private static string BuildImageKey(PersonImage image)
    {
        return BuildImageKey(image.Big, image.Med, image.Small, image.Thumb);
    }

    /// <summary>
    ///     Derives a shared image key from the provided URLs.
    /// </summary>
    /// <param name="big">URL for the big image.</param>
    /// <param name="med">URL for the medium image.</param>
    /// <param name="small">URL for the small image.</param>
    /// <param name="thumb">URL for the thumbnail image.</param>
    /// <returns>A deterministic key representing the image URLs.</returns>
    private static string BuildImageKey(string big, string med, string small, string thumb)
    {
        if (!string.IsNullOrWhiteSpace(thumb))
        {
            return $"thumb:{thumb}";
        }

        if (!string.IsNullOrWhiteSpace(small))
        {
            return $"small:{small}";
        }

        if (!string.IsNullOrWhiteSpace(med))
        {
            return $"med:{med}";
        }

        if (!string.IsNullOrWhiteSpace(big))
        {
            return $"big:{big}";
        }

        return "__empty__";
    }
}