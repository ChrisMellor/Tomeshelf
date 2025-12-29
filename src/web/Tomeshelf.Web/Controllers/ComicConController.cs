using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Controllers;

/// <summary>
///     Represents an ASP.NET Core MVC controller that handles requests related to ComicCon event guests.
/// </summary>
/// <param name="api">The API service used to retrieve guest information for ComicCon events. Cannot be null.</param>
[Route("comiccon")]
public class ComicConController(IGuestsApi api) : Controller
{
    /// <summary>
    ///     Handles HTTP GET requests to retrieve and display the list of guests for a specified event.
    /// </summary>
    /// <param name="eventId">
    ///     The unique identifier of the event for which to retrieve guest information. Cannot be null or
    ///     empty.
    /// </param>
    /// <param name="eventName">
    ///     An optional display name for the event. If not provided or empty, the event ID is used as the
    ///     display name.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>An IActionResult that renders the view displaying the grouped guest information for the specified event.</returns>
    [HttpGet("events/{eventId}/guests")]
    public async Task<IActionResult> Index([FromRoute] string eventId, [FromQuery] string? eventName, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = await api.GetComicConGuestsByEventResultAsync(eventId, cancellationToken);
        sw.Stop();

        ViewBag.EventName = string.IsNullOrWhiteSpace(eventName)
            ? eventId
            : eventName;
        ViewBag.Total = result.Total;
        ViewBag.ElapsedMs = sw.ElapsedMilliseconds;

        return View("Index", result.Groups);
    }
}