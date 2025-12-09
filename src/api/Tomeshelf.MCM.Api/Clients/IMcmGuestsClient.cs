using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Api.Records;

namespace Tomeshelf.MCM.Api.Clients;

/// <summary>
///     Defines a client for retrieving guest records associated with a specified city.
/// </summary>
/// <remarks>
///     Implementations of this interface are expected to provide asynchronous access to guest data for a
///     given city. Thread safety and error handling depend on the specific implementation.
/// </remarks>
public interface IMcmGuestsClient
{
    /// <summary>
    ///     Asynchronously retrieves a read-only list of guest records associated with the specified city.
    /// </summary>
    /// <param name="city">The city for which to fetch guest records. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a read-only list of guest records
    ///     for the specified city. If no guests are found, the list will be empty.
    /// </returns>
    Task<IReadOnlyList<GuestRecord>> FetchGuestsAsync(Guid eventId, CancellationToken cancellationToken);
}