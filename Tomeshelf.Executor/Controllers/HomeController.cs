using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Models;
using Tomeshelf.Executor.Services;

namespace Tomeshelf.Executor.Controllers;

public class HomeController : Controller
{
    private readonly IApiEndpointDiscoveryService _discovery;
    private readonly IExecutorSchedulerOrchestrator _scheduler;
    private readonly IExecutorConfigurationStore _store;

    public HomeController(IExecutorConfigurationStore store, IExecutorSchedulerOrchestrator scheduler, IApiEndpointDiscoveryService discovery)
    {
        _store = store;
        _scheduler = scheduler;
        _discovery = discovery;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var options = await _store.GetAsync(cancellationToken);

        return View(await CreateViewModelAsync(options, null, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(bool enabled, CancellationToken cancellationToken)
    {
        var options = await _store.GetAsync(cancellationToken);
        options.Enabled = enabled;
        await PersistAsync(options, cancellationToken);

        TempData["StatusMessage"] = enabled
                ? "Scheduler enabled."
                : "Scheduler disabled.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "Editor")] EndpointEditorModel model, CancellationToken cancellationToken)
    {
        var options = await _store.GetAsync(cancellationToken);

        if (!ModelState.IsValid)
        {
            return View("Index", await CreateViewModelAsync(options, model, cancellationToken));
        }

        if (options.Endpoints.Any(ep => string.Equals(ep.Name, model.Name, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(EndpointEditorModel.Name), "An endpoint with this name already exists.");

            return View("Index", await CreateViewModelAsync(options, model, cancellationToken));
        }

        options.Endpoints.Add(ToOptions(model));
        await PersistAsync(options, cancellationToken);

        TempData["StatusMessage"] = $"Added endpoint '{model.Name}'.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromRoute(Name = "name")] string? routeName, [FromForm(Name = "originalName")] string? originalName, [Bind(Prefix = "")] EndpointEditorModel model, CancellationToken cancellationToken)
    {
        var name = originalName ?? routeName;
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

        if (!string.Equals(name, model.Name, StringComparison.OrdinalIgnoreCase) && options.Endpoints.Any(ep => string.Equals(ep.Name, model.Name, StringComparison.OrdinalIgnoreCase)))
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

    [HttpPost]
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

    [HttpGet("discovery/endpoints")]
    public async Task<IActionResult> GetDiscoveredEndpoints([FromQuery] string baseUri, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(baseUri))
        {
            return BadRequest("Base URI is required.");
        }

        var endpoints = await _discovery.GetEndpointsAsync(baseUri, cancellationToken);
        var payload = endpoints.Select(e => new
        {
                id = e.Id,
                method = e.Method,
                relativePath = e.RelativePath,
                displayName = e.DisplayName,
                description = e.Description,
                allowBody = e.AllowBody,
                groupName = e.GroupName
        });

        return Ok(payload);
    }

    private async Task PersistAsync(ExecutorOptions options, CancellationToken cancellationToken)
    {
        await _store.SaveAsync(options, cancellationToken);
        await _scheduler.RefreshAsync(cancellationToken);
    }

    private async Task<ExecutorConfigurationViewModel> CreateViewModelAsync(ExecutorOptions options, EndpointEditorModel? editor, CancellationToken cancellationToken)
    {
        var apis = await _discovery.GetApisAsync(cancellationToken);

        return new ExecutorConfigurationViewModel
        {
                Enabled = options.Enabled,
                Endpoints = options.Endpoints.OrderBy(ep => ep.Name, StringComparer.OrdinalIgnoreCase)
                                   .Select(ToSummary)
                                   .ToList(),
                Editor = editor ??
                new EndpointEditorModel
                {
                        Enabled = true,
                        Method = "POST"
                },
                ApiServices = apis.Select(api => new ApiServiceOptionViewModel
                                   {
                                           ServiceName = api.ServiceName,
                                           DisplayName = api.DisplayName,
                                           BaseAddress = api.BaseAddress
                                   })
                                  .ToList()
        };
    }

    private static EndpointSummaryViewModel ToSummary(EndpointScheduleOptions endpoint)
    {
        var headersDisplay = endpoint.Headers is null || (endpoint.Headers.Count == 0)
                ? "None"
                : string.Join(", ", endpoint.Headers.Select(kvp => $"{kvp.Key}:{kvp.Value}"));

        return new EndpointSummaryViewModel
        {
                Name = endpoint.Name,
                Url = endpoint.Url,
                Method = endpoint.Method,
                Cron = endpoint.Cron,
                Enabled = endpoint.Enabled,
                HeadersDisplay = headersDisplay
        };
    }

    private static EndpointEditorModel ToEditorModel(EndpointScheduleOptions endpoint)
    {
        var headers = endpoint.Headers is null || (endpoint.Headers.Count == 0)
                ? null
                : string.Join(Environment.NewLine, endpoint.Headers.Select(kvp => $"{kvp.Key}:{kvp.Value}"));

        return new EndpointEditorModel
        {
                Name = endpoint.Name,
                Url = endpoint.Url,
                Method = endpoint.Method,
                Cron = endpoint.Cron,
                Enabled = endpoint.Enabled,
                Headers = headers
        };
    }

    private static EndpointScheduleOptions ToOptions(EndpointEditorModel model)
    {
        var endpoint = new EndpointScheduleOptions
        {
                Name = model.Name,
                Url = NormalizeUrl(model.Url),
                Method = model.Method,
                Cron = model.Cron,
                TimeZone = null,
                Enabled = model.Enabled,
                Headers = ParseHeaders(model.Headers)
        };

        return endpoint;
    }

    private static void UpdateEndpoint(EndpointScheduleOptions target, EndpointEditorModel model)
    {
        target.Name = model.Name;
        target.Url = NormalizeUrl(model.Url);
        target.Method = model.Method;
        target.Cron = model.Cron;
        target.TimeZone = null;
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

            var key = line[..separatorIndex]
                   .Trim();
            var value = line[(separatorIndex + 1)..]
                   .Trim();
            if (!string.IsNullOrWhiteSpace(key))
            {
                dictionary[key] = value;
            }
        }

        return dictionary;
    }

    private static string NormalizeUrl(string? url)
    {
        var trimmed = url?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return trimmed;
        }

        var candidate = trimmed.Contains("://", StringComparison.Ordinal)
                ? trimmed
                : $"http://{trimmed}";

        return Uri.TryCreate(candidate, UriKind.Absolute, out var uri)
                ? uri.ToString()
                : trimmed;
    }
}