using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.SHiFT.Application.Features.Redemption.Models;

namespace Tomeshelf.SHiFT.Application.Abstractions.External;

/// <summary>
///     Defines methods for managing user sessions, including authentication, CSRF token retrieval, and redemption
///     operations in a web application context.
/// </summary>
/// <remarks>
///     Implementations of this interface should ensure secure handling of user credentials and CSRF tokens
///     to prevent unauthorized access and cross-site request forgery attacks. All operations are asynchronous and support
///     cancellation via a cancellation token. This interface is intended for use in scenarios where session state and
///     security are critical, such as interacting with web-based reward or redemption systems.
/// </remarks>
public interface IShiftWebSession
{
    /// <summary>
    ///     Asynchronously builds a list of available redemption options for the specified code and service.
    /// </summary>
    /// <param name="code">The redemption code to evaluate. Cannot be null or empty.</param>
    /// <param name="csrfToken">The CSRF token used to validate the request. Cannot be null or empty.</param>
    /// <param name="service">
    ///     The name of the service for which redemption options are being requested. Cannot be null or
    ///     empty.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a list of available redemption
    ///     options for the specified code and service. The list will be empty if no options are available.
    /// </returns>
    Task<List<RedemptionOption>> BuildRedeemBodyAsync(string code, string csrfToken, string service, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously releases the unmanaged resources used by the object.
    /// </summary>
    /// <returns>A ValueTask that represents the asynchronous dispose operation.</returns>
    ValueTask DisposeAsync();

    /// <summary>
    ///     Asynchronously retrieves the CSRF token from the application's home page.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the CSRF token as a string.</returns>
    Task<string> GetCsrfFromHomeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously retrieves a CSRF token by making a request to the rewards endpoint.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the CSRF token as a string.</returns>
    Task<string> GetCsrfFromRewardsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously attempts to authenticate a user with the specified email address, password, and CSRF token.
    /// </summary>
    /// <param name="email">The email address of the user to authenticate. Cannot be null or empty.</param>
    /// <param name="password">The password associated with the specified email address. Cannot be null or empty.</param>
    /// <param name="csrfToken">
    ///     A CSRF (Cross-Site Request Forgery) token used to validate the authenticity of the login request. Cannot be null
    ///     or empty.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the login operation.</param>
    /// <returns>A task that represents the asynchronous login operation.</returns>
    Task LoginAsync(string email, string password, string csrfToken, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously redeems a code or token using the specified request body.
    /// </summary>
    /// <param name="redeemBody">The request body containing the code or token to redeem. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous redeem operation.</returns>
    Task RedeemAsync(string redeemBody, CancellationToken cancellationToke = default);
}