using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Contracts.SHiFT;

namespace Tomeshelf.Application.Abstractions.SHiFT;

/// <summary>
///     Defines methods for retrieving and updating shift-related settings and credentials.
/// </summary>
/// <remarks>
///     Implementations of this interface provide access to shift settings and associated credentials,
///     supporting asynchronous operations and cancellation. Methods allow clients to retrieve current settings, obtain
///     credentials for use, and update settings as needed.
/// </remarks>
public interface IShiftSettingsStore
{
    /// <summary>
    ///     Asynchronously retrieves the shift settings for the specified shift identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the shift whose settings are to be retrieved.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a <see cref="ShiftSettingsDto" />
    ///     with the settings for the specified shift, or <c>null</c> if no settings are found.
    /// </returns>
    Task<ShiftSettingsDto> GetAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    ///     Asynchronously retrieves a read-only list of email, password, and service tuples available for use.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a read-only list of tuples, each
    ///     containing an email address, password, and service name. The list is empty if no credentials are available.
    /// </returns>
    Task<IReadOnlyList<(string Email, string Password, string Service)>> GetForUseAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Creates a new shift settings entry or updates an existing one asynchronously based on the specified request.
    /// </summary>
    /// <param name="request">The shift settings update request containing the data to insert or update. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the upsert operation.</param>
    /// <returns>A task that represents the asynchronous upsert operation.</returns>
    Task UpsertAsync(ShiftSettingsUpdateRequest request, CancellationToken cancellationToken);
}