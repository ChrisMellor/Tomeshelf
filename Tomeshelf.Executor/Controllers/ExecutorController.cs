using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Models;
using Tomeshelf.Executor.Services;

namespace Tomeshelf.Executor.Controllers;

[Route("executor")]
public class ExecutorController(IExecutorConfigurationStore store,
                                IExecutorSchedulerOrchestrator scheduler,
                                ILogger<ExecutorController> logger) : Controller
{
    private readonly IExecutorConfigurationStore _store = store;
    private readonly IExecutorSchedulerOrchestrator _scheduler = scheduler;
    private readonly ILogger<ExecutorController> _logger = logger;

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var options = await _store.GetAsync(cancellationToken);
        return View(CreateViewModel(options));
    }

    [HttpPost("toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(bool enabled, CancellationToken cancellationToken)
    {
        var options = await _store.GetAsync(cancellationToken);
        options.Enabled = enabled;
        await PersistAsync(options, cancellationToken);

        TempData["StatusMessage"] = enabled ? "Scheduler enabled." : "Scheduler disabled.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EndpointEditorModel model, CancellationToken cancellationToken)
    {
        var options = await _store.GetAsync(cancellationToken);

        if (!ModelState.IsValid)
        {
            return View("Index", CreateViewModel(options, model));
        }

        if (options.Endpoints.Any(ep => string.Equals(ep.Name, model.Name, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(EndpointEditorModel.Name), "An endpoint with this name already exists.");
            return View("Index", CreateViewModel(options, model));
        }

        options.Endpoints.Add(ToOptions(model));
        await PersistAsync(options, cancellationToken);

        TempData["StatusMessage"] = $"Added endpoint '{model.Name}'.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{name}")]
    public async Task<IActionResult> Edit(string name, CancellationToken cancellationToken)
    {
        var options = await _store.GetAsync(cancellationToken);
        var endpoint = options.Endpoints.FirstOrDefault(ep => string.Equals(ep.Name, name, StringComparison.OrdinalIgnoreCase));
        if (endpoint is null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewData["OriginalName"] = endpoint.Name;
        return View(ToEditorModel(endpoint));
    }

    [HttpPost("edit/{name}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string name, EndpointEditorModel model, CancellationToken cancellationToken)
    {
        var options = await _store.GetAsync(cancellationToken);

        if (!ModelState.IsValid)
        {
            ViewData["OriginalName"] = name;
            return View(model);
        }

        var endpoint = options.Endpoints.FirstOrDefault(ep => string.Equals(ep.Name, name, StringComparison.OrdinalIgnoreCase));
        if (endpoint is null)
        {
            ModelState.AddModelError(string.Empty, "Endpoint not found.");
            ViewData["OriginalName"] = name;
            return View(model);
        }

        if (!string.Equals(name, model.Name, StringComparison.OrdinalIgnoreCase) &&
            options.Endpoints.Any(ep => string.Equals(ep.Name, model.Name, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(EndpointEditorModel.Name), "Another endpoint already uses this name.");
            ViewData["OriginalName"] = name;
            return View(model);
        }

        UpdateEndpoint(endpoint, model);
        await PersistAsync(options, cancellationToken);

        TempData["StatusMessage"] = $"Updated endpoint '{model.Name}'.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string name, CancellationToken cancellationToken)
    {
        var options = await _store.GetAsync(cancellationToken);
        var removed = options.Endpoints.RemoveAll(ep => string.Equals(ep.Name, name, StringComparison.OrdinalIgnoreCase));
        if (removed > 0)
        {
            await PersistAsync(options, cancellationToken);
            TempData["StatusMessage"] = $"Deleted endpoint '{name}'.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PersistAsync(ExecutorOptions options, CancellationToken cancellationToken)
    {
        await _store.SaveAsync(options, cancellationToken);
        await _scheduler.RefreshAsync(cancellationToken);
    }

    private static ExecutorConfigurationViewModel CreateViewModel(ExecutorOptions options, EndpointEditorModel? editor = null)
    {
        return new ExecutorConfigurationViewModel
        {
            Enabled = options.Enabled,
            Endpoints = options.Endpoints
                               .OrderBy(ep => ep.Name, StringComparer.OrdinalIgnoreCase)
                               .Select(ToSummary)
                               .ToList(),
            Editor = editor ?? new EndpointEditorModel { Enabled = true, Method = "POST" }
        };
    }

    private static EndpointSummaryViewModel ToSummary(EndpointScheduleOptions endpoint)
    {
        var headersDisplay = endpoint.Headers is null || endpoint.Headers.Count == 0
            ? "None"
            : string.Join(", ", endpoint.Headers.Select(kvp => $"{kvp.Key}:{kvp.Value}"));

        return new EndpointSummaryViewModel
        {
            Name = endpoint.Name,
            Url = endpoint.Url,
            Method = endpoint.Method,
            Cron = endpoint.Cron,
            TimeZone = endpoint.TimeZone,
            Enabled = endpoint.Enabled,
            HeadersDisplay = headersDisplay
        };
    }

    private static EndpointEditorModel ToEditorModel(EndpointScheduleOptions endpoint)
    {
        var headers = endpoint.Headers is null || endpoint.Headers.Count == 0
            ? null
            : string.Join(Environment.NewLine, endpoint.Headers.Select(kvp => $"{kvp.Key}:{kvp.Value}"));

        return new EndpointEditorModel
        {
            Name = endpoint.Name,
            Url = endpoint.Url,
            Method = endpoint.Method,
            Cron = endpoint.Cron,
            TimeZone = endpoint.TimeZone,
            Enabled = endpoint.Enabled,
            Headers = headers
        };
    }

    private static EndpointScheduleOptions ToOptions(EndpointEditorModel model)
    {
        var endpoint = new EndpointScheduleOptions
        {
            Name = model.Name,
            Url = model.Url,
            Method = model.Method,
            Cron = model.Cron,
            TimeZone = model.TimeZone,
            Enabled = model.Enabled,
            Headers = ParseHeaders(model.Headers)
        };

        return endpoint;
    }

    private static void UpdateEndpoint(EndpointScheduleOptions target, EndpointEditorModel model)
    {
        target.Name = model.Name;
        target.Url = model.Url;
        target.Method = model.Method;
        target.Cron = model.Cron;
        target.TimeZone = model.TimeZone;
        target.Enabled = model.Enabled;
        target.Headers = ParseHeaders(model.Headers);
    }

    private static Dictionary<string, string>? ParseHeaders(string? headers)
    {
        if (string.IsNullOrWhiteSpace(headers))
        {
            return null;
        }

        var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var lines = headers.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var separatorIndex = line.IndexOf(':');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();
            if (!string.IsNullOrWhiteSpace(key))
            {
                dictionary[key] = value;
            }
        }

        return dictionary;
    }
}
