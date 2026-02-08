using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tomeshelf.Fitbit.Application.Abstractions.Services;

public interface IFitbitDashboardService
{
    Task<FitbitDashboardDto> GetDashboardAsync(DateOnly date, bool forceRefresh = false, CancellationToken cancellationToken = default);
}