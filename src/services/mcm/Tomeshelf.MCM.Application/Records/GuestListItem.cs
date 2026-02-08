using System;

namespace Tomeshelf.MCM.Application.Records;

/// <summary>
///     Represents an entry in a guest list, including identifying information, profile details, and status indicators.
/// </summary>
/// <param name="Id">The unique identifier for the guest list item.</param>
/// <param name="Name">The display name of the guest.</param>
/// <param name="Description">A description or note associated with the guest.</param>
/// <param name="ProfileUrl">The URL to the guest's profile page. Can be null or empty if no profile is available.</param>
/// <param name="ImageUrl">The URL to the guest's profile image. Can be null or empty if no image is available.</param>
/// <param name="AddedAt">The date and time when the guest was added to the list, in UTC.</param>
/// <param name="RemovedAt">
///     The date and time when the guest was removed from the list, or null if the guest is currently
///     active.
/// </param>
/// <param name="IsDeleted">true if the guest list item has been marked as deleted; otherwise, false.</param>
public sealed record GuestListItem(Guid Id, string Name, string Description, string ProfileUrl, string ImageUrl, DateTimeOffset AddedAt, DateTimeOffset? RemovedAt, bool IsDeleted);