namespace Tomeshelf.Fitbit.Infrastructure;

internal sealed record AuthorizationState(string CodeVerifier, string ReturnUrl);
