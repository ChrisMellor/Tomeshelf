using System.Threading;
using System.Threading.Tasks;

namespace Tomeshelf.Application.Abstractions.SHiFT;

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
    ///     Attempts to redeem the specified SHiFT code asynchronously.
    /// </summary>
    /// <param name="shiftCode">The SHiFT code to redeem. Cannot be null or empty.</param>
    /// <param name="serviceOverride">
    ///     An optional service identifier to override the default redemption service. Specify null to use the default
    ///     service.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true" /> if the code was
    ///     redeemed successfully; otherwise, <see langword="false" />.
    /// </returns>
    Task<bool> RedeemCodeAsync(string shiftCode, string? serviceOverride, CancellationToken cancellationToken = default);
}