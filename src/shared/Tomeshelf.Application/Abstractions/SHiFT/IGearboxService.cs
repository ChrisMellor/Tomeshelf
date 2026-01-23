using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Contracts.SHiFT;

namespace Tomeshelf.Application.Shared.Abstractions.SHiFT;

/// <summary>
///     Defines methods for redeeming SHIFT codes using the Gearbox service.
/// </summary>
/// <remarks>
///     Implementations of this interface provide functionality to redeem SHIFT codes, which may grant
///     in-game rewards or benefits. Methods are asynchronous and support cancellation via a cancellation token.
/// </remarks>
public interface IGearboxService
{
    /// <summary>
    ///     Attempts to redeem the specified SHiFT code and returns the results for each applicable platform.
    /// </summary>
    /// <param name="shiftCode">The SHiFT code to redeem. Cannot be null or empty.</param>
    /// <param name="serviceOverride">
    ///     An optional service identifier to override the default redemption service. Specify null to use the default
    ///     service.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the redemption operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a read-only list of redemption
    ///     results, one for each platform the code was applied to. The list will be empty if the code could not be redeemed
    ///     on any platform.
    /// </returns>
    Task<IReadOnlyList<RedeemResult>> RedeemCodeAsync(string shiftCode, string? serviceOverride, CancellationToken cancellationToken = default);
}