using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Fitbit.Application.Features.Authorization.Models;

namespace Tomeshelf.Fitbit.Application.Features.Authorization.Queries;

public sealed record GetFitbitAuthorizationStatusQuery : IQuery<FitbitAuthorizationStatus>;