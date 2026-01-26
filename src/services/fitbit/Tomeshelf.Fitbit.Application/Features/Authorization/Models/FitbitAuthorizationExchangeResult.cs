namespace Tomeshelf.Fitbit.Application.Features.Authorization.Models;

public sealed record FitbitAuthorizationExchangeResult(bool IsInvalidState, string ReturnUrl);
