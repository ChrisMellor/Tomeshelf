using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Web.Models.Mcm;
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
    ///     Configs.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    [HttpGet("config")]
    public async Task<IActionResult> Config(CancellationToken cancellationToken = default)
    {
        try
        {
            var events = await api.GetComicConEventsAsync(cancellationToken, forceRefresh: true);

            return View("Config", new McmEventsConfigViewModel
            {
                Events = events
            });
        }
        catch (Exception ex)
        {
            return View("Config", new McmEventsConfigViewModel
            {
                ErrorMessage = $"Unable to load MCM event configuration: {ex.Message}"
            });
        }
    }

    /// <summary>
    ///     Inserts or updates.
    /// </summary>
    /// <param name="editor">The editor.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    [HttpPost("config/upsert")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upsert([Bind(Prefix = "Editor")] McmEventConfigEditorModel editor, CancellationToken cancellationToken = default)
    {
        editor.Id = (editor.Id ?? string.Empty).Trim();
        editor.Name = (editor.Name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(editor.Id))
        {
            ModelState.AddModelError("Editor.Id", "Event ID is required.");
        }

        if (string.IsNullOrWhiteSpace(editor.Name))
        {
            ModelState.AddModelError("Editor.Name", "Event name is required.");
        }

        if (!ModelState.IsValid)
        {
            var events = await SafeLoadEventsAsync(cancellationToken);

            return View("Config", new McmEventsConfigViewModel
            {
                Events = events,
                Editor = editor
            });
        }

        try
        {
            await api.UpsertComicConEventAsync(editor.Id, editor.Name, cancellationToken);

            TempData["StatusMessage"] = $"Saved event '{editor.Name}' ({editor.Id}).";

            return RedirectToAction(nameof(Config));
        }
        catch (Exception ex)
        {
            var events = await SafeLoadEventsAsync(cancellationToken);

            return View("Config", new McmEventsConfigViewModel
            {
                Events = events,
                Editor = editor,
                ErrorMessage = $"Failed to save event: {ex.Message}"
            });
        }
    }

    /// <summary>
    ///     Deletes.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    [HttpPost("config/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromForm] string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            TempData["StatusMessage"] = "Event ID is required.";

            return RedirectToAction(nameof(Config));
        }

        try
        {
            var deleted = await api.DeleteComicConEventAsync(id, cancellationToken);
            TempData["StatusMessage"] = deleted
                ? $"Deleted event '{id}'."
                : $"Event '{id}' not found.";
        }
        catch (Exception ex)
        {
            TempData["StatusMessage"] = $"Failed to delete event '{id}': {ex.Message}";
        }

        return RedirectToAction(nameof(Config));
    }

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

    /// <summary>
    ///     Safes the load events asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    private async Task<IReadOnlyList<McmEventConfigModel>> SafeLoadEventsAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await api.GetComicConEventsAsync(cancellationToken, forceRefresh: true);
        }
        catch
        {
            return Array.Empty<McmEventConfigModel>();
        }
    }
}
