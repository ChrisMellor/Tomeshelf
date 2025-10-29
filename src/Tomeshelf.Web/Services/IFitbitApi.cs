using System;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Services;

/// <summary>
///     Contract for retrieving Fitbit dashboard data from the backend API.
/// </summary>
public interface IFitbitApi
{
    /// <summary>
    ///     Retrieves the Fitbit dashboard snapshot for the specified date.
    /// </summary>
    /// <param name="date">Optional ISO date (yyyy-MM-dd). When null, the backend defaults to today.</param>
    /// <param name="refresh">When true, forces a refresh from the official Fitbit API.</param>
    /// <param name="returnUrl">Relative URL to redirect the user back to after successful authorization.</param>
    /// <param name="cancellationToken">Cancellation token for the HTTP request.</param>
    /// <returns>The dashboard payload or null when unavailable.</returns>
    Task<FitbitDashboardModel> GetDashboardAsync(string date, bool refresh, string returnUrl, CancellationToken cancellationToken);

    /// <summary>
    ///     Resolves the external Fitbit authorization URL by invoking the backend authorize endpoint.
    /// </summary>
    /// <param name="authorizeEndpoint">The backend API authorize endpoint returned when authorization is required.</param>
    /// <param name="cancellationToken">Cancellation token for the HTTP request.</param>
    /// <returns>An absolute Fitbit authorization URI.</returns>
    Task<Uri> ResolveAuthorizationAsync(Uri authorizeEndpoint, CancellationToken cancellationToken);
}