using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tomeshelf.Fitbit.Application.Abstractions.Services;

public interface IFitbitDashboardService
{
    /// <summary>
    ///     Gets the dashboard asynchronously.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="forceRefresh">The force refresh.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    Task<FitbitDashboardDto> GetDashboardAsync(DateOnly date, bool forceRefresh = false, CancellationToken cancellationToken = default);
}