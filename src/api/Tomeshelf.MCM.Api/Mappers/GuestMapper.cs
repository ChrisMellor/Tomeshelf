using System;
using System.Reflection;
using Tomeshelf.Domain.Entities.Mcm;

namespace Tomeshelf.Mcm.Api.Mappers;

/// <summary>
///     Provides methods for mapping, cloning, and updating guest entities, including utilities for generating guest
///     display
///     names and duplicating guest information for events.
/// </summary>
/// <remarks>
///     The GuestMapper class is intended for use in scenarios where guest data needs to be transformed,
///     cloned, or updated across different event contexts. All methods operate on the provided guest entities and do not
///     persist changes to a data store. Thread safety is not guaranteed; callers should ensure appropriate synchronization
///     if accessing shared instances concurrently.
/// </remarks>
public class GuestMapper : IGuestMapper
{
    /// <summary>
    ///     Returns the full name of the specified guest, combining the first and last names if available.
    /// </summary>
    /// <param name="guest">The guest entity from which to retrieve the full name. Cannot be null.</param>
    /// <returns>
    ///     A string containing the guest's full name, or an empty string if both first and last names are missing or
    ///     whitespace.
    /// </returns>
    public string GetGuestKey(GuestEntity guest)
    {
        var firstName = guest.Information?.FirstName?.Trim() ?? string.Empty;
        var lastName = guest.Information?.LastName?.Trim() ?? string.Empty;
        var fullName = $"{firstName} {lastName}".Trim();

        return string.IsNullOrWhiteSpace(fullName)
                ? string.Empty
                : fullName;
    }

    /// <summary>
    ///     Creates a new GuestEntity instance for the specified event by cloning the provided source guest's information.
    /// </summary>
    /// <remarks>
    ///     The cloned guest will have its IsDeleted property set to false, and new identifiers will be
    ///     generated for any missing or empty IDs in the source. The method does not modify the source instance.
    /// </remarks>
    /// <param name="eventId">The unique identifier of the event to associate with the cloned guest.</param>
    /// <param name="source">The GuestEntity instance to clone. Must not be null.</param>
    /// <returns>
    ///     A new GuestEntity instance containing the cloned information from the source, associated with the specified
    ///     event.
    /// </returns>
    public GuestEntity CloneForEvent(Guid eventId, GuestEntity source)
    {
        var guestId = source.Id != Guid.Empty
                ? source.Id
                : Guid.NewGuid();
        var infoId = source.Information?.Id != Guid.Empty
                ? source.Information!.Id
                : Guid.NewGuid();
        var socialId = source.Information?.Socials?.Id != Guid.Empty
                ? source.Information!.Socials!.Id
                : Guid.NewGuid();

        var sourceInfo = source.Information ?? new GuestInfoEntity();
        var sourceSocial = sourceInfo.Socials ?? new GuestSocial();

        var social = new GuestSocial
        {
            Id = socialId,
            GuestInfoId = infoId,
            Twitter = sourceSocial.Twitter,
            Facebook = sourceSocial.Facebook,
            Instagram = sourceSocial.Instagram,
            Imdb = sourceSocial.Imdb,
            YouTube = sourceSocial.YouTube,
            Twitch = sourceSocial.Twitch,
            Snapchat = sourceSocial.Snapchat,
            DeviantArt = sourceSocial.DeviantArt,
            Tumblr = sourceSocial.Tumblr,
            Fandom = sourceSocial.Fandom
        };

        var information = new GuestInfoEntity
        {
            Id = infoId,
            GuestId = guestId,
            FirstName = sourceInfo.FirstName,
            LastName = sourceInfo.LastName,
            Bio = sourceInfo.Bio,
            KnownFor = sourceInfo.KnownFor,
            Category = sourceInfo.Category,
            DaysAppearing = sourceInfo.DaysAppearing,
            ImageUrl = sourceInfo.ImageUrl,
            Socials = social
        };

        var guest = new GuestEntity
        {
            Id = guestId,
            EventId = eventId,
            GuestInfoId = infoId,
            IsDeleted = false,
            Information = information
        };

        return guest;
    }

    /// <summary>
    ///     Updates the properties of the specified target guest entity with values from the source guest entity.
    /// </summary>
    /// <remarks>
    ///     If the target entity is marked as deleted, this method restores it before applying updates.
    ///     The method ensures that the target entity and its related information and social properties are initialized as
    ///     needed. Only string properties are copied from the source to the target. No changes are made if the source
    ///     entity's Information property is null.
    /// </remarks>
    /// <param name="target">The guest entity to update. This object will be modified with values from the source entity.</param>
    /// <param name="source">
    ///     The guest entity containing the updated values to copy to the target entity. Must have a non-null Information
    ///     property.
    /// </param>
    /// <returns>true if any properties of the target entity were changed; otherwise, false.</returns>
    public bool UpdateGuest(GuestEntity target, GuestEntity source)
    {
        var sourceInfo = source.Information;
        if (sourceInfo is null)
        {
            return false;
        }

        var changed = false;

        if (target.IsDeleted)
        {
            target.IsDeleted = false;
            changed = true;
        }

        target.Information ??= new GuestInfoEntity
        {
            Id = Guid.NewGuid(),
            GuestId = target.Id
        };

        target.Information.Socials ??= new GuestSocial
        {
            Id = Guid.NewGuid(),
            GuestInfoId = target.Information.Id
        };

        changed |= CopyStringProperties(target.Information, sourceInfo);

        if (sourceInfo.Socials is not null)
        {
            changed |= CopyStringProperties(target.Information.Socials, sourceInfo.Socials);
        }

        return changed;
    }

    /// <summary>
    ///     Copies all public instance string property values from the source object to the target object if they differ,
    ///     using ordinal comparison.
    /// </summary>
    /// <remarks>
    ///     Only public instance properties of type string that are both readable and writable are
    ///     considered. Property values are compared using ordinal string comparison. Properties with equal values are not
    ///     updated.
    /// </remarks>
    /// <typeparam name="T">The type of the objects whose string properties are to be copied. Must be a reference type.</typeparam>
    /// <param name="target">The object whose string properties will be updated to match those of the source. Cannot be null.</param>
    /// <param name="source">The object from which string property values are copied. Cannot be null.</param>
    /// <returns>true if at least one string property value was changed on the target object; otherwise, false.</returns>
    private static bool CopyStringProperties<T>(T target, T source) where T : class
    {
        var changed = false;
        var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

        foreach (var property in properties)
        {
            if (!property.CanRead || !property.CanWrite || (property.PropertyType != typeof(string)))
            {
                continue;
            }

            var sourceValue = (string)property.GetValue(source);
            var targetValue = (string)property.GetValue(target);

            if (!string.Equals(targetValue, sourceValue, StringComparison.Ordinal))
            {
                property.SetValue(target, sourceValue);
                changed = true;
            }
        }

        return changed;
    }
}