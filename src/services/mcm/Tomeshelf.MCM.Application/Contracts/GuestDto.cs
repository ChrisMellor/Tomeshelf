using System;

namespace Tomeshelf.MCM.Application.Contracts;

/// <summary>
///     Represents a data transfer object containing information about a guest, including identification, descriptive
///     details, profile links, and status metadata.
/// </summary>
/// <param name="Id">The unique identifier of the guest.</param>
/// <param name="Name">The display name of the guest.</param>
/// <param name="Description">A brief description or biography of the guest.</param>
/// <param name="ProfileUrl">The URL to the guest's public profile or related page.</param>
/// <param name="ImageUrl">The URL of the guest's profile image.</param>
/// <param name="AddedAt">The date and time when the guest was added, expressed as a UTC timestamp.</param>
/// <param name="RemovedAt">The date and time when the guest was removed, or null if the guest is currently active.</param>
/// <param name="IsDeleted">true if the guest has been marked as deleted; otherwise, false.</param>
public sealed record GuestDto(Guid Id, string Name, string Description, string ProfileUrl, string ImageUrl, DateTimeOffset AddedAt, DateTimeOffset? RemovedAt, bool IsDeleted);