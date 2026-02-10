using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tomeshelf.Web.Models.Mcm;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.ViewComponents;

public sealed class ComicConEventsViewComponent : ViewComponent
{
    private readonly IGuestsApi _api;
    private readonly ILogger<ComicConEventsViewComponent> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ComicConEventsViewComponent" /> class.
    /// </summary>
    /// <param name="api">The api.</param>
    /// <param name="logger">The logger.</param>
    public ComicConEventsViewComponent(IGuestsApi api, ILogger<ComicConEventsViewComponent> logger)
    {
        _api = api;
        _logger = logger;
    }

    /// <summary>
    ///     Invokes asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public async Task<IViewComponentResult> InvokeAsync()
    {
        try
        {
            var events = await _api.GetComicConEventsAsync(HttpContext.RequestAborted);

            return View(events);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to load Comic Con events for the navbar.");

            return View(Array.Empty<McmEventConfigModel>());
        }
    }
}