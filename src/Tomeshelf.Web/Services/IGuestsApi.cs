using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Models.ComicCon;

namespace Tomeshelf.Web.Services;

/// <summary>
/// Abstraction for retrieving Comic Con guests from the API.
/// </summary>
public interface IGuestsApi
{
    /// <summary>
    /// Retrieves Comic Con guests for a given city from the API.
    /// </summary>
    /// <param name="city">City name to query.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A tuple of groups and total guests.</returns>
    /// <exception cref="HttpRequestException">Thrown when the HTTP response is unsuccessful.</exception>
    /// <exception cref="JsonException">Thrown when the response body cannot be parsed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the response payload is empty.</exception>
    Task<(IReadOnlyList<GuestsGroupModel> Groups, int Total)> GetComicConGuestsByCityAsync(string city, CancellationToken cancellationToken);
}
