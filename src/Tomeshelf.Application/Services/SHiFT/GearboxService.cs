using System;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Abstractions.SHiFT;

namespace Tomeshelf.Application.Services.SHiFT;

/// <summary>
///     Provides methods for redeeming SHiFT codes using configured user accounts and services.
/// </summary>
/// <remarks>
///     This class is intended for use with SHiFT-enabled games and services. It manages authentication and
///     code redemption workflows for all configured users. Instances of this class are thread-safe for concurrent
///     use.
/// </remarks>
public sealed class GearboxService : IGearboxService
{
    private readonly IShiftWebSession _session;
    private readonly IShiftSettingsStore _settings;

    /// <summary>
    ///     Initializes a new instance of the GearboxService class with the specified shift settings store and web session.
    /// </summary>
    /// <param name="settings">The shift settings store used to retrieve and persist shift configuration data. Cannot be null.</param>
    /// <param name="session">The web session associated with the current shift operation. Cannot be null.</param>
    public GearboxService(IShiftSettingsStore settings, IShiftWebSession session)
    {
        _settings = settings;
        _session = session;
    }

    /// <summary>
    ///     Attempts to redeem the specified SHiFT code for all configured user accounts using the provided or default
    ///     service.
    /// </summary>
    /// <param name="shiftCode">The SHiFT code to redeem. Cannot be null, empty, or consist only of white-space characters.</param>
    /// <param name="serviceOverride">
    ///     An optional service identifier to use when redeeming the code. If null or white space, the default service for
    ///     each user is used.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true" /> if the code
    ///     redemption process completes for all users.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown if <paramref name="shiftCode" /> is null, empty, or consists only of
    ///     white-space characters.
    /// </exception>
    public async Task<bool> RedeemCodeAsync(string shiftCode, string? serviceOverride, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(shiftCode))
        {
            throw new ArgumentException("SHiFT code is required.", nameof(shiftCode));
        }

        var users = await _settings.GetForUseAsync(cancellationToken);

        foreach (var (email, password, defaultService) in users)
        {
            var service = string.IsNullOrWhiteSpace(serviceOverride)
                ? defaultService
                : serviceOverride.Trim();

            var csrfHome = await _session.GetCsrfFromHomeAsync(cancellationToken);
            await _session.LoginAsync(email, password, csrfHome, cancellationToken);

            var csrfRewards = await _session.GetCsrfFromRewardsAsync(cancellationToken);

            var options = await _session.BuildRedeemBodyAsync(shiftCode.Trim(), csrfRewards, service, cancellationToken);

            foreach (var option in options)
            {
                await _session.RedeemAsync(option.FormBody, cancellationToken);
            }
        }

        return true;
    }
}