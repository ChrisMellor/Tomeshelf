using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.SHiFT.Application.Abstractions.External;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;
using Tomeshelf.SHiFT.Application.Features.Redemption.Models;
using Tomeshelf.SHiFT.Application.Features.Redemption.Redeem;

namespace Tomeshelf.SHiFT.Infrastructure.Services.External;

/// <summary>
///     Provides methods for managing and redeeming SHiFT codes for users, utilizing a specified settings store and web
///     session factory.
/// </summary>
/// <remarks>
///     The GearboxClient class is responsible for handling the redemption of SHiFT codes for all configured
///     users. It processes each user asynchronously and provides detailed results for each redemption attempt, including
///     success or failure information. The service requires a valid settings store and session factory to function
///     correctly.
/// </remarks>
public sealed class GearboxClient : IGearboxClient
{
    private readonly IShiftSettingsRepository _repository;
    private readonly IShiftWebSessionFactory _sessionFactory;

    /// <summary>
    ///     Initializes a new instance of the GearboxClient class with the specified shift repository store and web session
    ///     factory.
    /// </summary>
    /// <param name="repository">
    ///     The shift repository store used to retrieve and persist gearbox configuration data. Cannot be null.
    /// </param>
    /// <param name="sessionFactory">The factory used to create web sessions for shift operations. Cannot be null.</param>
    public GearboxClient(IShiftSettingsRepository repository, IShiftWebSessionFactory sessionFactory)
    {
        _repository = repository;
        _sessionFactory = sessionFactory;
    }

    /// <summary>
    ///     Attempts to redeem the specified SHiFT code for all configured users asynchronously.
    /// </summary>
    /// <remarks>
    ///     The method processes all users configured in the settings and attempts to redeem the provided
    ///     SHiFT code for each. If an error occurs for a user, the result for that user will indicate failure and include
    ///     error details. The operation is performed asynchronously and can be cancelled via the provided cancellation
    ///     token.
    /// </remarks>
    /// <param name="shiftCode">The SHiFT code to redeem. Cannot be null, empty, or consist only of white-space characters.</param>
    /// <param name="serviceOverride">
    ///     An optional service identifier to override the default service for each user. If null or white space, the user's
    ///     default service is used.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    ///     A read-only list of results indicating the outcome of the redemption attempt for each user. Each result contains
    ///     information about the user, the service used, and whether the redemption was successful.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown if <paramref name="shiftCode" /> is null, empty, or consists onl///
    ///     <exception cref="ArgumentException">exception>
    public async Task<IReadOnlyList<RedeemResult>> RedeemCodeAsync(string shiftCode, string? serviceOverride, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(shiftCode))
        {
            throw new ArgumentException("SHiFT code is required.", nameof(shiftCode));
        }

        var users = await _repository.GetForUseAsync(cancellationToken);

        var results = new List<RedeemResult>();

        foreach (var (id, email, password, defaultService) in users)
        {
            var service = string.IsNullOrWhiteSpace(serviceOverride)
                ? defaultService
                : serviceOverride.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(service))
            {
                results.Add(new RedeemResult(id, email, service, false, RedeemErrorCode.AccountMisconfigured, "Missing email, password, or service."));

                continue;
            }

            try
            {
                await using var session = _sessionFactory.Create();

                var csrfHome = await session.GetCsrfFromHomeAsync(cancellationToken);
                await session.LoginAsync(email, password, csrfHome, cancellationToken);

                var csrfRewards = await session.GetCsrfFromRewardsAsync(cancellationToken);

                var options = await session.BuildRedeemBodyAsync(shiftCode.Trim(), csrfRewards, service, cancellationToken);

                foreach (var option in options)
                {
                    await session.RedeemAsync(option.FormBody, cancellationToken);
                }

                results.Add(new RedeemResult(id, email, service, true, null, null));
            }
            catch (Exception ex)
            {
                var (code, message) = MapError(ex);
                results.Add(new RedeemResult(id, email, service, false, code, message));
            }
        }

        return results;
    }

    /// <summary>
    ///     Maps an exception to a corresponding redeem error code and user-friendly error message.
    /// </summary>
    /// <remarks>
    ///     Specific exception types and message patterns are mapped to known error codes. All other
    ///     exceptions are mapped to <see cref="RedeemErrorCode.Unknown" />. The returned message is intended for end-user
    ///     display and may be localized or customized as needed.
    /// </remarks>
    /// <param name="ex">The exception to evaluate and map to a redeem error code. Cannot be null.</param>
    /// <returns>
    ///     A tuple containing the mapped <see cref="RedeemErrorCode" /> and a descriptive error message suitable for display
    ///     to the user.
    /// </returns>
    private static (RedeemErrorCode Code, string Message) MapError(Exception ex)
    {
        RedeemErrorCode code;
        string message;

        switch (ex)
        {
            case InvalidOperationException ioe when ioe.Message.Contains("CSRF token not found", StringComparison.OrdinalIgnoreCase):
                code = RedeemErrorCode.CsrfMissing;
                message = "CSRF token not found.";

                break;
            case InvalidOperationException ioe when ioe.Message.Contains("No redemption form found", StringComparison.OrdinalIgnoreCase):
                code = RedeemErrorCode.NoRedemptionOptions;
                message = "No redemption options for that service.";

                break;
            case HttpRequestException:
            case TaskCanceledException:
                code = RedeemErrorCode.NetworkError;
                message = "Network error talking to SHiFT.";

                break;
            default:
                code = RedeemErrorCode.Unknown;
                message = "Unexpected error during redemption.";

                break;
        }

        return (code, message);
    }
}