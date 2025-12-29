namespace Tomeshelf.Mcm.Api.Records;

/// <summary>
///     Represents a guest with identifying and profile information, including name, description, and related URLs.
/// </summary>
/// <param name="Name">The full name of the guest. Cannot be null.</param>
/// <param name="Description">A brief description or biography of the guest. Cannot be null.</param>
/// <param name="ProfileUrl">The URL to the guest's profile page. Cannot be null.</param>
/// <param name="ImageUrl">The URL to an image representing the guest. Cannot be null.</param>
public sealed record GuestRecord(string Name, string Description, string ProfileUrl, string ImageUrl);