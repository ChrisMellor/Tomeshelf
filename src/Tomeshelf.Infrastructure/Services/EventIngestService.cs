using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Contracts;
using Tomeshelf.Domain.Entities.ComicCon;
using Tomeshelf.Infrastructure.Persistence;

namespace Tomeshelf.Infrastructure.Services;

/// <summary>
/// Handles ingesting event payloads into the database (upserts people, categories, schedules).
/// </summary>
/// <param name="context">EF Core database context.</param>
public class EventIngestService(TomeshelfDbContext context)
{
    /// <summary>
    /// Inserts or updates the event and all related people, categories, images and schedules.
    /// Creates new entities when missing and updates existing ones, then saves the changes.
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

    private async Task<Event> UpsertEventAsync(EventDto dto, CancellationToken ct)
    {
        var entity = await context.Events.SingleOrDefaultAsync(x => x.ExternalId == dto.EventId, ct);
        if (entity is null)
        {
            entity = new Event { ExternalId = dto.EventId, Name = dto.EventName, Slug = dto.EventSlug };
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

    private async Task<Dictionary<string, Category>> BuildCategoryCacheAsync(EventDto dto, CancellationToken ct)
    {
        var allCatIds = dto.People.SelectMany(p => p.GlobalCategories ?? [])
            .Select(c => c.Id).Distinct().ToList();
        var cache = await context.Categories
            .Where(c => allCatIds.Contains(c.ExternalId))
            .ToDictionaryAsync(c => c.ExternalId, c => c, ct);
        return cache;
    }

    private async Task<Person> GetOrCreatePersonAsync(string externalId, CancellationToken ct)
    {
        var person = await context.People
            .Include(x => x.Images)
            .Include(x => x.Categories).ThenInclude(pc => pc.Category)
            .SingleOrDefaultAsync(x => x.ExternalId == externalId, ct);
        if (person is null)
        {
            person = new Person { ExternalId = externalId };
            context.People.Add(person);
        }
        return person;
    }

    private static void UpdatePersonProperties(Person person, PersonDto data)
    {
        person.Uid = data.Uid;
        var wasVisible = person.PubliclyVisible;

        // Derive visibility: some sources signal cancellations via a global category named "Canceled"/"Cancelled".
        var isCanceledCategory = (data.GlobalCategories ?? [])
            .Any(c => !string.IsNullOrWhiteSpace(c.Name) &&
                      (string.Equals(c.Name.Trim(), "Canceled", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(c.Name.Trim(), "Cancelled", StringComparison.OrdinalIgnoreCase)));

        var desiredVisible = data.PubliclyVisible && !isCanceledCategory;
        person.PubliclyVisible = desiredVisible;

        if (!desiredVisible && person.RemovedUtc is null)
        {
            // Mark removal when transitioning from visible, or when a cancellation category is present on first ingest.
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

    private static void SyncPersonImages(Person person, List<ImageSetDto> images)
    {
        var desired = (images ?? [])
            .Select(img => (Key: BuildImageKey(img), Payload: img))
            .ToList();

        var existingGroups = person.Images
            .GroupBy(BuildImageKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => new Queue<PersonImage>(group), StringComparer.OrdinalIgnoreCase);

        foreach (var entry in desired)
        {
            if (existingGroups.TryGetValue(entry.Key, out var queue) && queue.Count > 0)
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

        var removals = existingGroups.Values
            .SelectMany(q => q)
            .ToList();
        foreach (var leftover in removals)
        {
            person.Images.Remove(leftover);
        }
    }

    private void EnsureCategories(List<CategoryDto> categories, Dictionary<string, Category> cache)
    {
        foreach (var c in categories ?? [])
        {
            GetOrAddCategory(c.Id, c.Name, cache, context);
        }
    }

    private static void SyncPersonCategories(Person person, List<CategoryDto> categories, Dictionary<string, Category> cache)
    {
        var desired = (categories ?? []).Select(c => c.Id).ToHashSet();
        person.Categories.RemoveWhere(link => !desired.Contains(link.Category.ExternalId));
        var existing = person.Categories.Select(x => x.Category.ExternalId).ToHashSet();
        foreach (var id in desired.Except(existing))
        {
            var category = cache[id];
            person.Categories.Add(new PersonCategory { Person = person, Category = category });
        }
    }

    private async Task<EventAppearance> GetOrCreateAppearanceAsync(Event eventEntity, Person person, CancellationToken ct)
    {
        EventAppearance? appearance = null;

        if (eventEntity.Id > 0 && person.Id > 0)
        {
            appearance = await context.EventAppearances
                .Include(x => x.Schedules)
                .SingleOrDefaultAsync(x => x.EventId == eventEntity.Id && x.PersonId == person.Id, ct);
        }

        if (appearance is null)
        {
            appearance = new EventAppearance { Event = eventEntity, Person = person };
            context.EventAppearances.Add(appearance);
        }

        return appearance;
    }

    private static void UpdateAppearanceProperties(EventAppearance a, PersonDto data)
    {
        a.DaysAtShow = data.DaysAtShow;
        a.BoothNumber = data.BoothNumber;
        a.AutographAmount = data.AutographAmount;
        a.PhotoOpAmount = data.PhotoOpAmount;
        a.PhotoOpTableAmount = data.PhotoOpTableAmount;
    }

    private async Task SyncSchedulesAsync(EventAppearance a, List<ScheduleDto> schedules, CancellationToken ct)
    {
        var byId = a.Schedules.ToDictionary(s => s.ExternalId);
        foreach (var sd in schedules ?? [])
        {
            if (!byId.TryGetValue(sd.Id, out var s))
            {
                s = new Schedule { EventAppearance = a, ExternalId = sd.Id };
                a.Schedules.Add(s);
            }

            s.Title = sd.Title;
            s.Description = sd.Description;
            s.StartTimeUtc = ParseAsUtc(sd.StartTime);
            s.EndTimeUtc = string.IsNullOrWhiteSpace(sd.EndTime) ? null : ParseAsUtc(sd.EndTime);
            s.NoEndTime = sd.NoEndTime;
            s.Location = sd.Location;

            if (sd.VenueLocation is not null)
            {
                var vl = await context.VenueLocations.SingleOrDefaultAsync(x => x.ExternalId == sd.VenueLocation.Id, ct);
                if (vl is null)
                {
                    vl = new VenueLocation { ExternalId = sd.VenueLocation.Id, Name = sd.VenueLocation.Name };
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
    /// Gets an existing category from the cache or creates and tracks a new one.
    /// Ensures the category name is updated to the latest value.
    /// </summary>
    /// <param name="id">External category identifier.</param>
    /// <param name="name">Current category display name.</param>
    /// <param name="categoryCache">Cache of categories indexed by external id.</param>
    /// <param name="dbContext">EF Core dbContext used to add new entities.</param>
    /// <returns>The resolved <see cref="Category"/>.</returns>
    private Category GetOrAddCategory(string id, string name, Dictionary<string, Category> categoryCache, TomeshelfDbContext dbContext)
    {
        if (categoryCache.TryGetValue(id, out var existing))
        {
            existing.Name = name;

            return existing;
        }

        var category = new Category { ExternalId = id, Name = name };
        dbContext.Categories.Add(category);
        categoryCache[id] = category;

        return category;
    }

    /// <summary>
    /// Parses a date-time string and returns the UTC DateTime.
    /// </summary>
    /// <param name="value">The date-time string representation.</param>
    /// <returns>A UTC <see cref="DateTime"/>.</returns>
    private static DateTime ParseAsUtc(string value)
    {
        var dto = DateTimeOffset.Parse(value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal);
        return dto.UtcDateTime;
    }

    private static string BuildImageKey(ImageSetDto image) =>
        BuildImageKey(image.Big, image.Med, image.Small, image.Thumb);

    private static string BuildImageKey(PersonImage image) =>
        BuildImageKey(image.Big, image.Med, image.Small, image.Thumb);

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
