namespace Tomeshelf.Fitbit.Application.Features.Authorization.Models;

public sealed record FitbitAuthorizationStatus(bool HasAccessToken, bool HasRefreshToken);
