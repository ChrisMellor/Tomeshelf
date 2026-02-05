using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tomeshelf.Fitbit.Application.Abstractions.Services;

public interface IFitbitAuthorizationService
{
    Uri BuildAuthorizationUri(string returnUrl, out string state);

    Task ExchangeAuthorizationCodeAsync(string code, string codeVerifier, CancellationToken cancellationToken);

    bool TryConsumeState(string state, out string codeVerifier, out string returnUrl);
}