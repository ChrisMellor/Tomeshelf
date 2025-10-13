using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Web.Models.Bundles;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Controllers;

[Route("bundles")]
public sealed class BundlesController(IBundlesApi api) : Controller
{
    /// <summary>
    ///     Displays Humble Bundle listings fetched from the backend API.
    /// </summary>
    /// <param name="includeExpired">Include expired bundles when true.</param>
    /// <param name="cancellationToken">Cancellation token for the API call.</param>
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] bool includeExpired = false,
        CancellationToken cancellationToken = default)
    {
        var bundles = await api.GetBundlesAsync(includeExpired, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var viewModels = bundles
            .Select(bundle =>
            {
                var timeRemaining = CalculateRemaining(bundle.EndsAt, now);
                var isExpired = bundle.EndsAt.HasValue && bundle.EndsAt.Value <= now;

                return new BundleViewModel
                {
                    MachineName = bundle.MachineName,
                    Category = bundle.Category,
                    Stamp = bundle.Stamp,
                    Title = string.IsNullOrWhiteSpace(bundle.Title)
                        ? bundle.ShortName ?? bundle.MachineName
                        : bundle.Title,
                    ShortName = bundle.ShortName,
                    Url = bundle.Url,
                    TileImageUrl = bundle.TileImageUrl,
                    TileLogoUrl = bundle.TileLogoUrl,
                    HeroImageUrl = bundle.HeroImageUrl,
                    ShortDescription = bundle.ShortDescription,
                    StartsAt = bundle.StartsAt,
                    EndsAt = bundle.EndsAt,
                    FirstSeenUtc = bundle.FirstSeenUtc,
                    LastSeenUtc = bundle.LastSeenUtc,
                    LastUpdatedUtc = bundle.LastUpdatedUtc,
                    IsExpired = isExpired,
                    TimeRemaining = isExpired ? null : timeRemaining,
                    SecondsRemaining = bundle.SecondsRemaining
                };
            })
            .ToList();

        var activeGroups = viewModels
            .Where(vm => !vm.IsExpired)
            .GroupBy(vm => Categorise(vm), StringComparer.OrdinalIgnoreCase)
            .Select(group => new BundlesCategoryGroup(
                group.Key,
                group
                    .OrderBy(vm => vm.EndsAt ?? DateTimeOffset.MaxValue)
                    .ThenBy(vm => vm.Title)
                    .ToList()))
            .OrderBy(g => g.Category)
            .ToList();

        var active = activeGroups;
        var expired = viewModels
            .Where(vm => vm.IsExpired)
            .OrderByDescending(vm => vm.EndsAt ?? vm.LastSeenUtc)
            .ThenBy(vm => vm.Title)
            .ToList();

        var dataTimestamp = bundles.Count > 0 ? bundles.Max(b => b.GeneratedUtc) : now;

        var model = new BundlesIndexViewModel
        {
            ActiveBundles = active,
            ExpiredBundles = expired,
            IncludeExpired = includeExpired,
            DataTimestampUtc = dataTimestamp
        };

        return View(model);
    }

    private static TimeSpan? CalculateRemaining(DateTimeOffset? endsAt, DateTimeOffset now)
    {
        if (!endsAt.HasValue) return null;

        var remaining = endsAt.Value - now;
        return remaining > TimeSpan.Zero ? remaining : null;
    }

    private static string Categorise(BundleViewModel vm)
    {
        return string.IsNullOrWhiteSpace(vm.Category)
            ? string.IsNullOrWhiteSpace(vm.Stamp) ? "Other" : Capitalize(vm.Stamp)
            : Capitalize(vm.Category);
    }

    private static string Capitalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "Other";

        return char.ToUpperInvariant(value[0]) + value[1..];
    }
}