using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace Tomeshelf.ServiceDefaults;

/// <summary>
///     Extension methods for mapping Executor discovery endpoints.
/// </summary>
public static class ExecutorDiscoveryExtensions
{
    /// <summary>
    ///     Maps a discovery endpoint that enumerates MVC actions for Executor.
    /// </summary>
    /// <param name="app">Endpoint route builder.</param>
    /// <param name="pattern">Optional custom path.</param>
    /// <returns>The supplied endpoint route builder for chaining.</returns>
    public static IEndpointRouteBuilder MapExecutorDiscoveryEndpoint(this IEndpointRouteBuilder app, string? pattern = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        var resolvedPattern = string.IsNullOrWhiteSpace(pattern)
            ? ExecutorDiscoveryConstants.DefaultPath
            : pattern.Trim();
        if (!resolvedPattern.StartsWith('/'))
        {
            resolvedPattern = "/" + resolvedPattern;
        }

        app.MapGet(resolvedPattern, (IApiDescriptionGroupCollectionProvider provider, IHostEnvironment environment) =>
            {
                var serviceName = environment?.ApplicationName ?? "unknown";
                var endpoints = provider.ApiDescriptionGroups
                                        .Items
                                        .SelectMany(g => g.Items)
                                        .Where(d => d is not null && !string.IsNullOrWhiteSpace(d.RelativePath) && !string.IsNullOrWhiteSpace(d.HttpMethod))
                                        .Select(CreateEndpoint)
                                        .Where(static e => e is not null)
                                        .GroupBy(e => $"{e.Method}:{e.RelativePath}", StringComparer.OrdinalIgnoreCase)
                                        .Select(g => g.First())
                                        .OrderBy(e => e.RelativePath, StringComparer.OrdinalIgnoreCase)
                                        .ThenBy(e => e.Method, StringComparer.OrdinalIgnoreCase)
                                        .ToList();

                return Results.Ok(new ExecutorDiscoveryDocument(serviceName, endpoints));
            })
           .WithName("executor-discovery")
           .AllowAnonymous()
           .ExcludeFromDescription();

        return app;
    }

    private static bool AllowsBody(ApiDescription description)
    {
        if ((description.HttpMethod?.Equals("GET", StringComparison.OrdinalIgnoreCase) == true) || (description.HttpMethod?.Equals("DELETE", StringComparison.OrdinalIgnoreCase) == true) || (description.HttpMethod?.Equals("HEAD", StringComparison.OrdinalIgnoreCase) == true))
        {
            if (!description.ParameterDescriptions.Any(static p => (p.Source == BindingSource.Body) || (p.Source == BindingSource.Form)))
            {
                return false;
            }
        }

        if (description.SupportedRequestFormats?.Count > 0)
        {
            return true;
        }

        return description.ParameterDescriptions.Any(static p => (p.Source == BindingSource.Body) || (p.Source == BindingSource.Form) || (p.Source == BindingSource.FormFile));
    }

    private static ExecutorDiscoveredEndpoint? CreateEndpoint(ApiDescription description)
    {
        var method = description.HttpMethod?.ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(method))
        {
            return null;
        }

        var relativePath = "/" + description.RelativePath!.TrimStart('/');

        var displayName = description.ActionDescriptor?.DisplayName;
        var descriptionText = description.ActionDescriptor
                                        ?.EndpointMetadata
                                        ?.OfType<ProducesResponseTypeAttribute>()
                                        ?.FirstOrDefault()
                                        ?.StatusCode switch
        {
            StatusCodes.Status200OK => "Returns 200 OK",
            StatusCodes.Status202Accepted => "Returns 202 Accepted",
            StatusCodes.Status204NoContent => "Returns 204 No Content",
            _ => null
        };

        var allowBody = AllowsBody(description);

        var id = description.ActionDescriptor?.AttributeRouteInfo?.Name;
        if (string.IsNullOrWhiteSpace(id))
        {
            id = $"{method}:{relativePath}";
        }

        var groupName = description.GroupName;
        if (string.IsNullOrWhiteSpace(groupName) && description.ActionDescriptor?.RouteValues is not null && description.ActionDescriptor.RouteValues.TryGetValue("controller", out var controllerName) && !string.IsNullOrWhiteSpace(controllerName))
        {
            groupName = controllerName;
        }

        return new ExecutorDiscoveredEndpoint(id, method, relativePath, displayName, descriptionText, allowBody, groupName);
    }
}