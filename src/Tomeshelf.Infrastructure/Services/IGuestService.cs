using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Contracts;

namespace Tomeshelf.Infrastructure.Services;

/// <summary>
/// Abstraction for retrieving and persisting event guests by city.
/// </summary>
public interface IGuestService
{
    /// <summary>
    /// Retrieves and persists the latest guests for the specified city.
    /// </summary>
    /// <param name="city">City name to query (must be configured).</param>
    /// <returns>The list of people returned by the external API.</returns>
    /// <exception cref="ApplicationException">Thrown when the city is not configured or no guests are returned.</exception>
    /// <exception cref="HttpRequestException">Thrown when the external request fails.</exception>
    Task<List<PersonDto>> GetGuestsAsync(string city, CancellationToken cancellationToken = default);
}
