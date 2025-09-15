using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Tomeshelf.Application.Contracts;

using System.Threading;

namespace Tomeshelf.Infrastructure.Clients;

/// <summary>
/// Abstraction over the external Comic Con People API client.
/// </summary>
public interface IGuestsClient
{
    /// <summary>
    /// Fetches the latest Comic Con event guests for the provided key.
    /// </summary>
    /// <param name="key">The event/city API key.</param>
    /// <returns>The event DTO, or null if no body was returned.</returns>
    /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
    /// <exception cref="Exception">Thrown when the API returns a non-success status code.</exception>
    /// <exception cref="JsonException">Thrown when the response body cannot be parsed.</exception>
    Task<EventDto> GetLatestGuestsAsync(Guid key, CancellationToken cancellationToken = default);
}
