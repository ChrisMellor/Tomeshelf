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
    ///     Retrieves the Fitbit overview snapshot plus 7/30 day history for the specified date.
    /// </summary>
    /// <param name="date">Optional ISO date (yyyy-MM-dd). When null, the backend defaults to today.</param>
    /// <param name="refresh">When true, forces a refresh from the official Fitbit API.</param>
    /// <param name="returnUrl">Relative URL to redirect the user back to after successful authorization.</param>
    /// <param name="cancellationToken">Cancellation token for the HTTP request.</param>
    /// <returns>The overview payload or null when unavailable.</returns>
    Task<FitbitOverviewModel> GetOverviewAsync(string date, bool refresh, string returnUrl, CancellationToken cancellationToken);

}
