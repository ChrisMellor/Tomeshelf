using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Controllers;

[Route("comiccon")]
public class ComicConController(IGuestsApi api) : Controller
{
    /// <summary>
    /// Displays Comic Con guests for the specified city.
    /// Fetches grouped guests from the API and renders the Index view.
    /// </summary>
    /// <param name="city">City name to query (e.g., "London").</param>
    /// <param name="cancellationToken">Cancellation token for the HTTP call.</param>
    /// <returns>An <see cref="IActionResult"/> that renders the guests view.</returns>
    [HttpGet("city/{city}/guests")]
    public async Task<IActionResult> Index([FromRoute] string city, CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = await api.GetComicConGuestsByCityResultAsync(city, cancellationToken);
        sw.Stop();

        ViewBag.City = city;
        ViewBag.Total = result.Total;
        ViewBag.WarmingUp = result.WarmingUp;
        ViewBag.ElapsedMs = sw.ElapsedMilliseconds;

        return View("Index", result.Groups);
    }


}
