using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tomeshelf.Fitbit.Application.Abstractions.Services;

public interface IFitbitAuthorizationService
{
    /// <summary>
    ///     Builds the authorization uri.
    /// </summary>
    /// <param name="returnUrl">The return url.</param>
    /// <param name="state">The state.</param>
    /// <returns>The result of the operation.</returns>
    Uri BuildAuthorizationUri(string returnUrl, out string state);

    /// <summary>
    ///     Exchanges the authorization code asynchronously.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <param name="codeVerifier">The code verifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExchangeAuthorizationCodeAsync(string code, string codeVerifier, CancellationToken cancellationToken);

    /// <summary>
    ///     Attempts to consume a state.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="codeVerifier">The code verifier.</param>
    /// <param name="returnUrl">The return url.</param>
    /// <returns>True if the condition is met; otherwise, false.</returns>
    bool TryConsumeState(string state, out string codeVerifier, out string returnUrl);
}