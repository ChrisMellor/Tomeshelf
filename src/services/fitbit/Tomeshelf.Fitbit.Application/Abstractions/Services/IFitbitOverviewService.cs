using System;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Fitbit.Application.Features.Overview.Models;

namespace Tomeshelf.Fitbit.Application.Abstractions.Services;

public interface IFitbitOverviewService
{
    /// <summary>
    ///     Gets the overview asynchronously.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="forceRefresh">The force refresh.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    Task<FitbitOverviewDto> GetOverviewAsync(DateOnly date, bool forceRefresh, CancellationToken cancellationToken);
}