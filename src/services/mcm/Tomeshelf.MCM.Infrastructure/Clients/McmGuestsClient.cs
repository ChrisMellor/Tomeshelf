#nullable enable

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tomeshelf.MCM.Application.Abstractions.Clients;
using Tomeshelf.MCM.Application.Records;
using Tomeshelf.MCM.Infrastructure.Responses;

namespace Tomeshelf.MCM.Infrastructure.Clients;

/// <summary>
///     Provides methods for retrieving guest records associated with events from the MCM API.
/// </summary>
/// <remarks>
///     This client is intended for use with the MCM (Movie Comic Media) API to fetch information about event
///     guests. It requires a configured HTTP client and logger to function. Instances of this class are not thread-safe
///     for
///     concurrent method calls unless external synchronization is provided.
/// </remarks>
public class McmGuestsClient : IMcmGuestsClient
{
    public const string HttpClientName = "Mcm.Guests";
    private readonly HttpClient _httpClient;
    private readonly ILogger<McmGuestsClient> _logger;

    /// <summary>
    ///     Initializes a new instance of the McmGuestsClient class with the specified HTTP client and logger.
    /// </summary>
    /// <param name="httpClientFactory">Factory used to resolve the named HTTP client for the MCM Guests API. Must not be null.</param>
    /// <param name="logger">
    ///     The logger used to record diagnostic and operational information for the McmGuestsClient. Must not
    ///     be null.
    /// </param>
    public McmGuestsClient(IHttpClientFactory httpClientFactory, ILogger<McmGuestsClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient(HttpClientName);
        _logger = logger;
    }

    /// <summary>
    ///     Asynchronously retrieves the list of guests associated with the specified event.
    /// </summary>
    /// <remarks>
    ///     This method sends a request to an external API to obtain guest information for the given event. The
    ///     operation may take time to complete depending on network conditions. The returned list is read-only and should not
    ///     be modified.
    /// </remarks>
    /// <param name="eventId">The unique identifier of the event for which to fetch guest records. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A read-only list of guest records for the specified event. The list will be empty if no guests are found.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the API response payload is empty or does not include guest
    ///     information.
    /// </exception>
    public async Task<IReadOnlyList<GuestRecord>> FetchGuestsAsync(string eventId, CancellationToken cancellationToken)
    {
        var requestUri = $"api/people?key={Uri.EscapeDataString(eventId)}";

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = new StringContent("{}", Encoding.UTF8, "application/json") };

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var safeEventId = eventId?.Replace("\r", string.Empty)
                                      .Replace("\n", string.Empty);

            _logger.LogWarning("MCM API returned {StatusCode} for event {EventId}.", response.StatusCode, safeEventId);
            response.EnsureSuccessStatusCode();
        }

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new InvalidOperationException("MCM API returned an empty payload.");
        }

        var responseModel = JsonConvert.DeserializeObject<McmEventResponse>(payload);
        if (responseModel?.People is null)
        {
            throw new InvalidOperationException("MCM API payload did not include people.");
        }

        return MapGuests(responseModel.People);
    }

    /// <summary>
    ///     Builds a display name for the specified person using available name fields.
    /// </summary>
    /// <remarks>
    ///     Leading and trailing whitespace is trimmed from each name component before constructing the
    ///     display name.
    /// </remarks>
    /// <param name="person">The person whose name information is used to construct the display name. Cannot be null.</param>
    /// <returns>
    ///     A string containing the person's full name if both first and last names are available; otherwise, returns the
    ///     first name, last name, or alternative name, in that order of preference. Returns an empty string if no name
    ///     information is available.
    /// </returns>
    internal static string BuildName(McmEventResponse.Person person)
    {
        var firstName = person.FirstName?.Trim();
        var lastName = person.LastName?.Trim();

        if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
        {
            return $"{firstName} {lastName}";
        }

        if (!string.IsNullOrWhiteSpace(firstName))
        {
            return firstName;
        }

        if (!string.IsNullOrWhiteSpace(lastName))
        {
            return lastName;
        }

        return person.AltName?.Trim() ?? string.Empty;
    }

    /// <summary>
    ///     Returns the first string in the specified list that is not null, empty, or consists only of white-space characters,
    ///     with leading and trailing white space removed.
    /// </summary>
    /// <param name="values">
    ///     An array of strings to search for the first non-empty value. Elements may be null or contain only white-space
    ///     characters.
    /// </param>
    /// <returns>
    ///     The first non-empty, non-white-space string from the input array, trimmed of leading and trailing white space; or
    ///     null if all values are null, empty, or white space.
    /// </returns>
    internal static string FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return string.Empty;
    }

    /// <summary>
    ///     Maps an array of person data from an event response to a read-only list of guest records, filtering out invalid
    ///     or incomplete entries.
    /// </summary>
    /// <remarks>
    ///     Entries in the input array that are null or lack a valid name are excluded from the result.
    ///     The returned list does not contain null elements and preserves the order of valid guests as found in the
    ///     input.
    /// </remarks>
    /// <param name="people">
    ///     An array of person objects representing potential guests to be mapped. Entries may be null or contain incomplete
    ///     data.
    /// </param>
    /// <returns>
    ///     A read-only list of guest records corresponding to valid people in the input array. The list is empty if no
    ///     valid guests are found.
    /// </returns>
    internal static IReadOnlyList<GuestRecord> MapGuests(McmEventResponse.Person?[] people)
    {
        if (people.Length == 0)
        {
            return [];
        }

        var records = new List<GuestRecord>(people.Length);
        foreach (var person in people)
        {
            if (person is null)
            {
                continue;
            }

            var name = BuildName(person);
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var description = !string.IsNullOrWhiteSpace(person.Bio)
                ? person.Bio
                : person.KnownFor;

            var profileUrl = PickProfileUrl(person);
            var imageUrl = PickImageUrl(person.Images);

            records.Add(new GuestRecord(name, description ?? string.Empty, profileUrl, imageUrl));
        }

        return records;
    }

    /// <summary>
    ///     Selects the first available non-empty image URL from the provided array of images, prioritizing larger image
    ///     sizes.
    /// </summary>
    /// <remarks>
    ///     The method checks each image in the order provided and returns the first non-empty URL,
    ///     preferring larger image sizes (Big, then Med, then Small, then Thumb) for each image. If all images are null or
    ///     contain only empty URLs, the method returns null.
    /// </remarks>
    /// <param name="images">An array of image objects to search for a valid image URL. The array can be null or empty.</param>
    /// <returns>A string containing the first non-empty image URL found, or null if no valid URL is available.</returns>
    internal static string PickImageUrl(McmEventResponse.Image?[]? images)
    {
        if (images is null || (images.Length == 0))
        {
            return string.Empty;
        }

        foreach (var image in images)
        {
            if (image is null)
            {
                continue;
            }

            var url = FirstNonEmpty(image.Big, image.Med, image.Small, image.Thumb);
            if (!string.IsNullOrWhiteSpace(url))
            {
                return url;
            }
        }

        return string.Empty;
    }

    /// <summary>
    ///     Selects the most relevant profile URL for the specified person from a prioritized list of known social and media
    ///     platforms.
    /// </summary>
    /// <remarks>
    ///     The method checks the person's primary profile URL first, followed by common platforms such
    ///     as IMDb, Twitter, Instagram, Facebook, YouTube, Twitch, TikTok, Fandom, DeviantArt, Tumblr, and Snapchat, in
    ///     that order. This is useful for linking to a person's most prominent online presence.
    /// </remarks>
    /// <param name="person">The person whose profile URLs are to be evaluated. Cannot be null.</param>
    /// <returns>A string containing the first non-empty profile URL found for the person, or null if none are available.</returns>
    internal static string PickProfileUrl(McmEventResponse.Person person)
    {
        return FirstNonEmpty(person.ProfileUrl, person.Imdb, person.Twitter, person.Instagram, person.Facebook, person.YouTube, person.Twitch, person.TikTok, person.Fandom, person.DeviantArt, person.Tumblr, person.Snapchat);
    }
}
