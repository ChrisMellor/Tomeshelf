using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Mcm.Api.Records;

namespace Tomeshelf.Mcm.Api.Clients;

/// <summary>
///     Provides methods for retrieving guest records for a specified city.
/// </summary>
public class McmGuestsClient : IMcmGuestsClient
{
    /// <summary>
    ///     Asynchronously retrieves the list of guests associated with the specified event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event for which to fetch guest records.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a read-only list of guest records for
    ///     the specified event. The list will be empty if no guests are found.
    /// </returns>
    public async Task<IReadOnlyList<GuestRecord>> FetchGuestsAsync(Guid eventId, CancellationToken cancellationToken)
    {
        // Deterministic fake data per city so you can test sync deltas.
        var guests = new ReadOnlyCollection<GuestRecord>(new List<GuestRecord>());

        return guests;
    }
}