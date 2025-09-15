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
        var entity = await context.Events.SingleOrDefaultAsync(x => x.ExternalId == eventData.EventId, cancellationToken);
        if (entity is null)
        {
            entity = new Event { ExternalId = eventData.EventId, Name = eventData.EventName, Slug = eventData.EventSlug };
            context.Events.Add(entity);
        }
        else
        {
            entity.Name = eventData.EventName;
            entity.Slug = eventData.EventSlug;
            entity.UpdatedUtc = DateTime.UtcNow;
        }

        var allCatIds = eventData.People
            .SelectMany(personData => personData.GlobalCategories ?? [])
            .Select(categoryData => categoryData.Id)
            .Distinct()
            .ToList();

        var categoryCache = await context.Categories
            .Where(c => allCatIds.Contains(c.ExternalId))
            .ToDictionaryAsync(c => c.ExternalId, c => c, cancellationToken);

        foreach (var personData in eventData.People)
        {
            var person = await context.People
                .Include(x => x.Images)
                .Include(x => x.Categories).ThenInclude(pc => pc.Category)
                .SingleOrDefaultAsync(x => x.ExternalId == personData.Id, cancellationToken);

            if (person is null)
            {
                person = new Person { ExternalId = personData.Id };
                context.People.Add(person);
            }

            person.Uid = personData.Uid;
            person.PubliclyVisible = personData.PubliclyVisible;
            person.FirstName = personData.FirstName ?? "";
            person.LastName = personData.LastName ?? "";
            person.AltName = personData.AltName;
            person.Bio = personData.Bio;
            person.KnownFor = personData.KnownFor;
            person.ProfileUrl = personData.ProfileUrl;
            person.ProfileUrlLabel = personData.ProfileUrlLabel;
            person.VideoLink = personData.VideoLink;
            person.Twitter = personData.Twitter;
            person.Facebook = personData.Facebook;
            person.Instagram = personData.Instagram;
            person.YouTube = personData.YouTube;
            person.Twitch = personData.Twitch;
            person.Snapchat = personData.Snapchat;
            person.DeviantArt = personData.DeviantArt;
            person.Tumblr = personData.Tumblr;
            person.UpdatedUtc ??= DateTime.UtcNow;

            person.Images.Clear();
            foreach (var img in personData.Images ?? [])
            {
                person.Images.Add(new PersonImage
                {
                    Big = img.Big,
                    Med = img.Med,
                    Small = img.Small,
                    Thumb = img.Thumb
                });
            }

            var desiredCatIds = (personData.GlobalCategories ?? [])
                .Select(categoryData => categoryData.Id)
                .ToHashSet();

            foreach (var categoryData in personData.GlobalCategories ?? [])
            {
                GetOrAddCategory(categoryData.Id, categoryData.Name, categoryCache, context);
            }

            person.Categories.RemoveWhere(link => !desiredCatIds.Contains(link.Category.ExternalId));
            var existingCatIds = person.Categories.Select(x => x.Category.ExternalId).ToHashSet();

            foreach (var id in desiredCatIds.Except(existingCatIds))
            {
                var category = categoryCache[id];
                person.Categories.Add(new PersonCategory { Person = person, Category = category });
            }

            var eventAppearance = await context.EventAppearances
                .Include(x => x.Schedules)
                .SingleOrDefaultAsync(x => x.EventId == entity.Id && x.PersonId == person.Id, cancellationToken);

            if (eventAppearance is null)
            {
                eventAppearance = new EventAppearance { Event = entity, Person = person };
                context.EventAppearances.Add(eventAppearance);
            }

            eventAppearance.DaysAtShow = personData.DaysAtShow;
            eventAppearance.BoothNumber = personData.BoothNumber;
            eventAppearance.AutographAmount = personData.AutographAmount;
            eventAppearance.PhotoOpAmount = personData.PhotoOpAmount;
            eventAppearance.PhotoOpTableAmount = personData.PhotoOpTableAmount;

            var scheduleById = eventAppearance.Schedules.ToDictionary(s => s.ExternalId);
            foreach (var scheduleData in personData.Schedules ?? [])
            {
                if (!scheduleById.TryGetValue(scheduleData.Id, out var schedule))
                {
                    schedule = new Schedule { EventAppearance = eventAppearance, ExternalId = scheduleData.Id };
                    eventAppearance.Schedules.Add(schedule);
                }

                schedule.Title = scheduleData.Title;
                schedule.Description = scheduleData.Description;
                schedule.StartTimeUtc = ParseAsUtc(scheduleData.StartTime);
                schedule.EndTimeUtc = string.IsNullOrWhiteSpace(scheduleData.EndTime) ? null : ParseAsUtc(scheduleData.EndTime);
                schedule.NoEndTime = scheduleData.NoEndTime;
                schedule.Location = scheduleData.Location;

                if (scheduleData.VenueLocation is not null)
                {
                    var venueLocation = await context.VenueLocations.SingleOrDefaultAsync(vl => vl.ExternalId == scheduleData.VenueLocation.Id, cancellationToken);
                    if (venueLocation is null)
                    {
                        venueLocation = new VenueLocation { ExternalId = scheduleData.VenueLocation.Id, Name = scheduleData.VenueLocation.Name };
                        context.VenueLocations.Add(venueLocation);
                    }
                    else
                    {
                        venueLocation.Name = scheduleData.VenueLocation.Name;
                    }
                    schedule.VenueLocation = venueLocation;
                }
                else
                {
                    schedule.VenueLocation = null;
                }
            }
        }

        return await context.SaveChangesAsync(cancellationToken);
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
    /// Parses a date-time string and forces it to UTC kind.
    /// </summary>
    /// <param name="value">The date-time string representation.</param>
    /// <returns>A <see cref="DateTime"/> with <see cref="DateTimeKind.Utc"/>.</returns>
    private static DateTime ParseAsUtc(string value)
    {
        var dateTime = DateTime.SpecifyKind(DateTime.Parse(value), DateTimeKind.Utc);

        return dateTime;
    }
}
