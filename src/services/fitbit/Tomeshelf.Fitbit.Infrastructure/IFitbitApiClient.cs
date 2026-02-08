using System;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Fitbit.Infrastructure.Models;

namespace Tomeshelf.Fitbit.Infrastructure;

public interface IFitbitApiClient
{
    Task<ActivitiesResponse> GetActivitiesAsync(DateOnly date, CancellationToken cancellationToken);

    Task<FoodLogSummaryResponse> GetCaloriesInAsync(DateOnly date, CancellationToken cancellationToken);

    Task<SleepResponse> GetSleepAsync(DateOnly date, CancellationToken cancellationToken);

    Task<WeightResponse> GetWeightAsync(DateOnly date, int lookbackDays, CancellationToken cancellationToken);
}