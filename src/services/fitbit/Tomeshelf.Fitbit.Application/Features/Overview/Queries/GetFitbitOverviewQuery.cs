using System;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Fitbit.Application.Features.Overview.Models;

namespace Tomeshelf.Fitbit.Application.Features.Overview.Queries;

public sealed record GetFitbitOverviewQuery(DateOnly Date, bool ForceRefresh) : IQuery<FitbitOverviewDto>;
