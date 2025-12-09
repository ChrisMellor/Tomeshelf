using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Mcm.Api.Records;

namespace Tomeshelf.Mcm.Api.Clients;

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
    ///     Asynchronously retrieves the list of guests associated with the specified event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event for which to fetch guest records.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a read-only list of guest records for
    ///     the specified event. If no guests are found, the list will be empty.
    /// </returns>
    Task<IReadOnlyList<GuestRecord>> FetchGuestsAsync(Guid eventId, CancellationToken cancellationToken);
}