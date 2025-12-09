using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Api.Records;

namespace Tomeshelf.MCM.Api.Clients;

/// <summary>
///     Provides methods for retrieving guest records for a specified city.
/// </summary>
public class McmGuestsClient : IMcmGuestsClient
{
    /// <summary>
    ///     Asynchronously retrieves the list of guest records for the specified city.
    /// </summary>
    /// <param name="city">The city for which to fetch guest records.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a read-only list of guest records
    ///     for the specified city. The list is empty if there are no guests for the city.
    /// </returns>
    public async Task<IReadOnlyList<GuestRecord>> FetchGuestsAsync(Guid eventId, CancellationToken cancellationToken)
    {
        // Deterministic fake data per city so you can test sync deltas.
        var guests = new ReadOnlyCollection<GuestRecord>(new List<GuestRecord>());

        return guests;
    }
}