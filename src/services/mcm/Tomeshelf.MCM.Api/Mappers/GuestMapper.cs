using System;
using System.Reflection;
using Tomeshelf.Domain.Shared.Entities.Mcm;

namespace Tomeshelf.Mcm.Api.Mappers;

/// <summary>
///     Provides methods for cloning, updating, and retrieving information about guest entities, including mapping guest
///     data between different event contexts.
/// </summary>
/// <remarks>
///     The GuestMapper class is typically used to manage guest entity data within event management systems,
///     enabling operations such as cloning guest records for new events, updating guest details, and generating display
///     keys. All methods assume that input entities are valid and may throw exceptions if null arguments are provided.
///     This
///     class is not thread-safe.
/// </remarks>
public class GuestMapper : IGuestMapper
{
    /// <summary>
    ///     Creates a new GuestEntity instance for the specified event by cloning the provided source guest entity.
    /// </summary>
    /// <remarks>
    ///     The cloned guest entity will have the same information and social details as the source, but will be
    ///     associated with the provided event identifier. If the source or its nested entities do not have valid identifiers,
    ///     new unique identifiers will be generated as needed. The IsDeleted property of the cloned entity is set to
    ///     false.
    /// </remarks>
    /// <param name="eventId">The identifier of the event to associate with the cloned guest entity.</param>
    /// <param name="source">The GuestEntity instance to clone. Must not be null.</param>
    /// <returns>
    ///     A new GuestEntity instance associated with the specified event, containing copied information from the source
    ///     entity.
    /// </returns>
    public GuestEntity CloneForEvent(string eventId, GuestEntity source)
    {
        var guestId = source.Id != Guid.Empty
            ? source.Id
            : Guid.NewGuid();

        var infoId = source.Information?.Id != Guid.Empty
            ? source.Information!.Id
            : Guid.NewGuid();

        var sourceInfo = source.Information ?? new GuestInfoEntity();
        var sourceSocial = sourceInfo.Socials;

        GuestSocial social = null;

        if (sourceSocial is not null)
        {
            var socialId = sourceSocial.Id != Guid.Empty
                ? sourceSocial.Id
                : Guid.NewGuid();

            social = new GuestSocial
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
        }

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
    ///     Generates a display key for the specified guest based on their first and last name.
    /// </summary>
    /// <param name="guest">The guest entity from which to retrieve the display name. Cannot be null.</param>
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
    ///     Updates the specified guest entity with information from another guest entity.
    /// </summary>
    /// <remarks>
    ///     This method copies relevant properties from the source entity to the target entity, including
    ///     nested information and social details. If the source entity's Information property is null, no updates are
    ///     performed and false is returned. The method ensures that identifiers and relationships between the guest and its
    ///     information are consistent after the update.
    /// </remarks>
    /// <param name="target">The guest entity to update. This object will be modified with values from the source entity.</param>
    /// <param name="source">
    ///     The guest entity containing the updated information to apply to the target entity. Must have a non-null
    ///     Information property.
    /// </param>
    /// <returns>true if any changes were made to the target entity; otherwise, false.</returns>
    public bool UpdateGuest(GuestEntity target, GuestEntity source)
    {
        var sourceInfo = source.Information;
        if (sourceInfo is null)
        {
            return false;
        }

        var changed = false;
        var sourceSocial = sourceInfo.Socials;

        if (target.IsDeleted)
        {
            target.IsDeleted = false;
            if (target.RemovedAt is not null)
            {
                target.RemovedAt = null;
            }

            if (target.AddedAt == default)
            {
                target.AddedAt = DateTimeOffset.UtcNow;
            }

            changed = true;
        }

        if (target.Information is null)
        {
            var infoId = target.GuestInfoId != Guid.Empty ? target.GuestInfoId :
                sourceInfo.Id != Guid.Empty ? sourceInfo.Id : Guid.NewGuid();

            target.Information = new GuestInfoEntity
            {
                Id = infoId,
                GuestId = target.Id,
                Socials = null
            };

            target.GuestInfoId = infoId;
            changed = true;
        }
        else
        {
            if (target.Information.Id == Guid.Empty)
            {
                target.Information.Id = target.GuestInfoId != Guid.Empty ? target.GuestInfoId :
                    sourceInfo.Id != Guid.Empty ? sourceInfo.Id : Guid.NewGuid();
                changed = true;
            }

            if (target.GuestInfoId == Guid.Empty)
            {
                target.GuestInfoId = target.Information.Id;
                changed = true;
            }

            if (target.Information.GuestId != target.Id)
            {
                target.Information.GuestId = target.Id;
                changed = true;
            }
        }

        changed |= CopyStringProperties(target.Information, sourceInfo);

        if (sourceSocial is null)
        {
            if (target.Information.Socials is not null)
            {
                target.Information.Socials = null;
                changed = true;
            }
        }
        else
        {
            target.Information.Socials ??= new GuestSocial
            {
                Id = sourceSocial.Id != Guid.Empty
                    ? sourceSocial.Id
                    : Guid.NewGuid(),
                GuestInfoId = target.Information.Id
            };

            if (target.Information.Socials.Id == Guid.Empty)
            {
                target.Information.Socials.Id = sourceSocial.Id != Guid.Empty
                    ? sourceSocial.Id
                    : Guid.NewGuid();
                changed = true;
            }

            if (target.Information.Socials.GuestInfoId != target.Information.Id)
            {
                target.Information.Socials.GuestInfoId = target.Information.Id;
                changed = true;
            }

            changed |= CopyStringProperties(target.Information.Socials, sourceSocial);
        }

        return changed;
    }

    /// <summary>
    ///     Copies all public instance string property values from the source object to the target object if they differ, using
    ///     ordinal comparison.
    /// </summary>
    /// <remarks>
    ///     Only public instance properties of type string that are both readable and writable are considered.
    ///     Property values are compared using ordinal string comparison. Properties are set on the target only if the values
    ///     differ. Both target and source must be of the same type.
    /// </remarks>
    /// <typeparam name="T">The type of the objects whose string properties are to be copied. Must be a reference type.</typeparam>
    /// <param name="target">
    ///     The object whose string property values will be updated to match those of the source object. Must
    ///     not be null.
    /// </param>
    /// <param name="source">The object from which string property values are copied. Must not be null.</param>
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

            var sourceValue = property.GetValue(source) as string;
            var targetValue = property.GetValue(target) as string;

            if (!string.Equals(targetValue, sourceValue, StringComparison.Ordinal))
            {
                property.SetValue(target, sourceValue);
                changed = true;
            }
        }

        return changed;
    }
}