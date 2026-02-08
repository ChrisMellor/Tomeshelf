using System;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Fitbit.Application.Features.Overview.Models;

namespace Tomeshelf.Fitbit.Application.Abstractions.Services;

public interface IFitbitOverviewService
{
    Task<FitbitOverviewDto> GetOverviewAsync(DateOnly date, bool forceRefresh, CancellationToken cancellationToken);
}