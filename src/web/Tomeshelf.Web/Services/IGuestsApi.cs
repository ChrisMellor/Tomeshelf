using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Models;
using Tomeshelf.Web.Models.ComicCon;
using Tomeshelf.Web.Models.Mcm;

namespace Tomeshelf.Web.Services;

/// <summary>
///     Defines methods for retrieving Comic Con guest and event information from the API.
/// </summary>
/// <remarks>
///     Implementations of this interface provide asynchronous access to guest lists and event configurations
///     for Comic Con events. Methods may throw exceptions if the API response is unsuccessful or cannot be parsed. All
///     operations support cancellation via a provided token.
/// </remarks>
public interface IGuestsApi
{
    /// <summary>
    ///     Asynchronously retrieves a read-only list of Comic Con event configurations.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a read-only list of Comic Con event
    ///     configuration models. The list will be empty if no events are available.
    /// </returns>
    Task<IReadOnlyList<McmEventConfigModel>> GetComicConEventsAsync(CancellationToken cancellationToken, bool forceRefresh = false);

    Task UpsertComicConEventAsync(string eventId, string name, CancellationToken cancellationToken);

    /// <returns><see langword="true" /> if the event was deleted; <see langword="false" /> if it does not exist.</returns>
    Task<bool> DeleteComicConEventAsync(string eventId, CancellationToken cancellationToken);

    /// <summary>
    ///     Asynchronously retrieves the list of guest groups and the total number of guests for a specified Comic Con
    ///     event.
    /// </summary>
    /// <param name="eventId">
    ///     The unique identifier of the Comic Con event for which to retrieve guest information. Cannot be
    ///     null or empty.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a tuple with a read-only list of
    ///     guest groups and the total number of guests for the specified event.
    /// </returns>
    Task<(IReadOnlyList<GuestsGroupModel> Groups, int Total)> GetComicConGuestsByEventAsync(string eventId, CancellationToken cancellationToken);

    /// <summary>
    ///     Asynchronously retrieves the list of guests attending a specified Comic Con event.
    /// </summary>
    /// <param name="eventId">
    ///     The unique identifier of the Comic Con event for which to retrieve guest information. Cannot be
    ///     null or empty.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a <see cref="GuestsByEventResult" />
    ///     object with details about the guests for the specified event.
    /// </returns>
    Task<GuestsByEventResult> GetComicConGuestsByEventResultAsync(string eventId, CancellationToken cancellationToken);
}
