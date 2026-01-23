using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Api.Records;

namespace Tomeshelf.MCM.Api.Clients;

/// <summary>
///     Defines methods for retrieving guest information associated with events.
/// </summary>
public interface IMcmGuestsClient
{
    /// <summary>
    ///     Asynchronously retrieves the list of guests associated with the specified event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event for which to fetch guest records. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a read-only list of guest records for
    ///     the specified event. The list is empty if no guests are found.
    /// </returns>
    Task<IReadOnlyList<GuestRecord>> FetchGuestsAsync(string eventId, CancellationToken cancellationToken);
}