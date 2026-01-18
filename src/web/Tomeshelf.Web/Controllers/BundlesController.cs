using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Infrastructure;
using Tomeshelf.Web.Models.Bundles;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Controllers;

[Route("bundles")]
public sealed class BundlesController(IBundlesApi api, IFileUploadsApi uploadsApi) : Controller
{
    private const string LastViewedCookieName = "tomeshelf_bundles_lastViewedUtc";
    private readonly IBundlesApi _api = api;
    private readonly IFileUploadsApi _uploadsApi = uploadsApi;

    /// <summary>
    ///     Displays Humble Bundle listings fetched from the backend API.
    /// </summary>
    /// <param name="includeExpired">Include expired bundles when true.</param>
    /// <param name="cancellationToken">Cancellation token for the API call.</param>
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] bool includeExpired = false, CancellationToken cancellationToken = default)
    {
        DateTimeOffset? lastViewed = null;
        if (Request.Cookies.TryGetValue(LastViewedCookieName, out var cookieValue) && DateTimeOffset.TryParse(cookieValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
        {
            lastViewed = parsed;
        }

        var bundles = await _api.GetBundlesAsync(includeExpired, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var viewModels = bundles.Select(bundle =>
                                 {
                                     var timeRemaining = CalculateRemaining(bundle.EndsAt, now);
                                     var isExpired = bundle.EndsAt.HasValue && (bundle.EndsAt.Value <= now);
                                     var isNew = lastViewed.HasValue && (bundle.FirstSeenUtc > lastViewed.Value);
                                     var isUpdated = lastViewed.HasValue && (bundle.LastUpdatedUtc > lastViewed.Value);

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
                                         TimeRemaining = isExpired
                                             ? null
                                             : timeRemaining,
                                         SecondsRemaining = bundle.SecondsRemaining,
                                         IsNewSinceLastFetch = isNew,
                                         IsUpdatedSinceLastFetch = isUpdated
                                     };
                                 })
                                .ToList();

        var activeGroups = viewModels.Where(vm => !vm.IsExpired)
                                     .GroupBy(vm => Categorise(vm), StringComparer.OrdinalIgnoreCase)
                                     .Select(group => new BundlesCategoryGroup(group.Key, group.OrderBy(vm => vm.EndsAt ?? DateTimeOffset.MaxValue)
                                                                                               .ThenBy(vm => vm.Title)
                                                                                               .ToList()))
                                     .OrderBy(g => g.Category)
                                     .ToList();

        var active = activeGroups;
        var expired = viewModels.Where(vm => vm.IsExpired)
                                .OrderByDescending(vm => vm.EndsAt ?? vm.LastSeenUtc)
                                .ThenBy(vm => vm.Title)
                                .ToList();

        var dataTimestamp = bundles.Count > 0
            ? bundles.Max(b => b.GeneratedUtc)
            : now;

        if (bundles.Count > 0)
        {
            Response.Cookies.Append(LastViewedCookieName, dataTimestamp.ToString("O"), new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(14),
                HttpOnly = false,
                IsEssential = true
            });
        }

        var model = new BundlesIndexViewModel
        {
            ActiveBundles = active,
            ExpiredBundles = expired,
            IncludeExpired = includeExpired,
            DataTimestampUtc = dataTimestamp,
            LastViewedUtc = lastViewed,
            NewBundlesCount = viewModels.Count(vm => vm.IsNewSinceLastFetch),
            UpdatedBundlesCount = viewModels.Count(vm => vm.IsUpdatedSinceLastFetch && !vm.IsNewSinceLastFetch)
        };

        return View(model);
    }

    /// <summary>
    ///     Shows a form for uploading a Humble Bundle archive to Google Drive.
    /// </summary>
    [HttpGet("upload")]
    public IActionResult Upload()
    {
        var hasTokens = HasDriveTokens();

        return View(new BundleUploadViewModel
        {
            Error = hasTokens
                ? null
                : "Google Drive is not authorised yet. Run the OAuth flow first."
        });
    }

    /// <summary>
    ///     Handles bundle upload form submissions and forwards the archive to the backend API.
    /// </summary>
    /// <param name="archive">Zip archive containing the bundle files.</param>
    /// <param name="cancellationToken">Cancellation token for the API call.</param>
    [HttpPost("upload")]
    [RequestSizeLimit(1_073_741_824)] // ~1GB
    [RequestFormLimits(MultipartBodyLengthLimit = 1_073_741_824)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload([FromForm] IFormFile archive, CancellationToken cancellationToken = default)
    {
        if (archive is null || (archive.Length == 0))
        {
            return View(new BundleUploadViewModel
            {
                Error = "Please choose a Humble Bundle zip archive to upload."
            });
        }

        try
        {
            var auth = GetDriveAuth();
            if (auth is null)
            {
                return View(new BundleUploadViewModel
                {
                    Error = "Google Drive is not authorised. Please run the OAuth flow first."
                });
            }

            await using var stream = archive.OpenReadStream();
            var result = await _uploadsApi.UploadBundleAsync(stream, archive.FileName, auth, cancellationToken);

            return View(new BundleUploadViewModel
            {
                Result = result
            });
        }
        catch (Exception ex)
        {
            return View(new BundleUploadViewModel
            {
                Error = $"Upload failed: {ex.Message}"
            });
        }
    }

    private static TimeSpan? CalculateRemaining(DateTimeOffset? endsAt, DateTimeOffset now)
    {
        if (!endsAt.HasValue)
        {
            return null;
        }

        var remaining = endsAt.Value - now;

        return remaining > TimeSpan.Zero
            ? remaining
            : null;
    }

    private static string Capitalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Other";
        }

        return char.ToUpperInvariant(value[0]) + value[1..];
    }

    private static string Categorise(BundleViewModel vm)
    {
        return string.IsNullOrWhiteSpace(vm.Category)
            ? string.IsNullOrWhiteSpace(vm.Stamp)
                ? "Other"
                : Capitalize(vm.Stamp)
            : Capitalize(vm.Category);
    }

    private GoogleDriveAuthModel? GetDriveAuth()
    {
        var clientId = HttpContext.Session.GetString(GoogleDriveSessionKeys.ClientId);
        var clientSecret = HttpContext.Session.GetString(GoogleDriveSessionKeys.ClientSecret);
        var refreshToken = HttpContext.Session.GetString(GoogleDriveSessionKeys.RefreshToken);
        var userEmail = HttpContext.Session.GetString(GoogleDriveSessionKeys.UserEmail);

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        return new GoogleDriveAuthModel
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            RefreshToken = refreshToken,
            UserEmail = userEmail
        };
    }

    private bool HasDriveTokens()
    {
        return !string.IsNullOrWhiteSpace(HttpContext.Session.GetString(GoogleDriveSessionKeys.ClientId)) && !string.IsNullOrWhiteSpace(HttpContext.Session.GetString(GoogleDriveSessionKeys.ClientSecret)) && !string.IsNullOrWhiteSpace(HttpContext.Session.GetString(GoogleDriveSessionKeys.RefreshToken));
    }
}