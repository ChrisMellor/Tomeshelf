using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Contracts.SHiFT;

namespace Tomeshelf.Application.Shared.Abstractions.SHiFT;

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
    ///     Asynchronously creates a new shift settings entry using the specified update request.
    /// </summary>
    /// <param name="request">An object containing the details of the shift settings to create. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the unique identifier of the newly
    ///     created shift settings entry.
    /// </returns>
    Task<int> CreateAsync(ShiftSettingsUpdateRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);

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
    ///     Asynchronously retrieves a read-only list of credential records available for use.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a read-only list of tuples, each
    ///     containing the identifier, email, password, and service name for a credential. The list will be empty if no
    ///     credentials are available.
    /// </returns>
    Task<IReadOnlyList<(int Id, string Email, string Password, string Service)>> GetForUseAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Asynchronously updates the shift settings for the specified shift identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the shift to update.</param>
    /// <param name="request">An object containing the updated shift settings. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    Task UpdateAsync(int id, ShiftSettingsUpdateRequest request, CancellationToken cancellationToken);
}