using System;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Fitbit.Infrastructure.Models;

namespace Tomeshelf.Fitbit.Infrastructure;

public interface IFitbitApiClient
{
    /// <summary>
    ///     Gets the activities asynchronously.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    Task<ActivitiesResponse> GetActivitiesAsync(DateOnly date, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the calories in asynchronously.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    Task<FoodLogSummaryResponse> GetCaloriesInAsync(DateOnly date, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the sleep asynchronously.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    Task<SleepResponse> GetSleepAsync(DateOnly date, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the weight asynchronously.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="lookbackDays">The lookback days.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    Task<WeightResponse> GetWeightAsync(DateOnly date, int lookbackDays, CancellationToken cancellationToken);
}