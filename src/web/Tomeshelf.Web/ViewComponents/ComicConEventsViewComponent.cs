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

    public ComicConEventsViewComponent(IGuestsApi api, ILogger<ComicConEventsViewComponent> logger)
    {
        _api = api;
        _logger = logger;
    }

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
