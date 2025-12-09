namespace Tomeshelf.Mcm.Api.Records;

/// <summary>
///     Represents a guest profile for the MCM system, including name, description, profile URL, and image URL.
/// </summary>
/// <param name="Name">The full name of the guest.</param>
/// <param name="Description">A brief description of the guest, such as their role or background.</param>
/// <param name="ProfileUrl">The URL to the guest's public profile or related page. Can be null or empty if not available.</param>
/// <param name="ImageUrl">The URL to an image representing the guest. Can be null or empty if not available.</param>
public sealed record GuestRecord(string Name, string Description, string ProfileUrl, string ImageUrl);