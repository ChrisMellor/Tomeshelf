using System;
using Tomeshelf.Application.Shared.Abstractions.Messaging;

namespace Tomeshelf.Fitbit.Application.Features.Dashboard.Queries;

public sealed record GetFitbitDashboardQuery(DateOnly Date, bool ForceRefresh) : IQuery<FitbitDashboardDto>;