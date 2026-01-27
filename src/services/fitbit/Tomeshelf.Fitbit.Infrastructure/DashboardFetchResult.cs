using Tomeshelf.Fitbit.Application;

namespace Tomeshelf.Fitbit.Infrastructure;

internal sealed record DashboardFetchResult(FitbitDashboardDto Snapshot, FetchStatus Status);
