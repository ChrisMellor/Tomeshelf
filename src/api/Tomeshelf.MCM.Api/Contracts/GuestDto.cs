namespace Tomeshelf.MCM.Api.Contracts;

/// <summary>
///     Represents a guest with identifying information, description, and profile resources.
/// </summary>
/// <param name="Name">The name of the guest. Cannot be null or empty.</param>
/// <param name="Description">
///     A brief description of the guest, such as their role or background. Can be empty if no
///     description is available.
/// </param>
/// <param name="ProfileUrl">
///     The URL to the guest's public profile or website. Can be null or empty if no profile is
///     available.
/// </param>
/// <param name="ImageUrl">The URL to an image representing the guest. Can be null or empty if no image is available.</param>
public sealed record GuestDto(string Name, string Description, string ProfileUrl, string ImageUrl);