using System;

namespace Tomeshelf.Fitbit.Application.Features.Authorization.Models;

public sealed record FitbitAuthorizationRedirect(Uri AuthorizationUri, string State);
