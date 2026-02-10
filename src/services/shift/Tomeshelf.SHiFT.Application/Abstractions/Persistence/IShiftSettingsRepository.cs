using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.SHiFT.Domain.Entities;

namespace Tomeshelf.SHiFT.Application.Abstractions.Persistence;

/// <summary>
///     Defines a contract for managing shift settings, including creating, retrieving, updating, and deleting shift
///     settings entries asynchronously.
/// </summary>
/// <remarks>
///     Implementations of this interface should ensure thread safety and support cancellation via the
///     provided tokens. The interface enables efficient management of shift-related configurations and credential records,
///     facilitating scalable and responsive operations in multi-user or distributed environments.
/// </remarks>
public interface IShiftSettingsRepository
{
    /// <summary>
    ///     Asynchronously creates a new shift settings entry using the specified update entity.
    /// </summary>
    /// <param name="entity">An object containing the details of the shift settings to create. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the unique identifier of the newly
    ///     created shift settings entry.
    /// </returns>
    Task<int> CreateAsync(SettingsEntity entity, CancellationToken cancellationToken);

    /// <summary>
    ///     Asynchronously deletes the entity identified by the specified unique identifier.
    /// </summary>
    /// <remarks>Throws an exception if no entity with the specified identifier exists.</remarks>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    ///     Determines whether the specified email address exists in the system, optionally excluding a user by ID from the
    ///     check.
    /// </summary>
    /// <remarks>
    ///     This method performs an asynchronous operation, which may involve a database call. The
    ///     operation can be cancelled using the provided cancellation token.
    /// </remarks>
    /// <param name="email">The email address to check for existence. This parameter cannot be null or empty.</param>
    /// <param name="excludingId">
    ///     An optional user ID to exclude from the existence check. If specified, the method ignores this user when
    ///     determining if the email exists.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains <see langword="true" /> if the email
    ///     address exists; otherwise, <see langword="false" />.
    /// </returns>
    Task<bool> EmailExistsAsync(string email, int? excludingId, CancellationToken cancellationToken);

    /// <summary>
    ///     Asynchronously retrieves the shift settings for the specified shift identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the shift whose settings are to be retrieved.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a <see cref="SettingsEntity" />
    ///     with the settings for the specified shift, or <c>null</c> if no settings are found.
    /// </returns>
    Task<SettingsEntity?> GetByIdAsync(int id, CancellationToken cancellationToken);

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
    ///     Asynchronously retrieves all configured SHiFT settings records.
    /// </summary>
    /// <remarks>
    ///     This method is intended for configuration management experiences where accounts need to be listed for
    ///     editing/deletion. Passwords remain encrypted in the returned entities.
    /// </remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of settings entities.</returns>
    Task<IReadOnlyList<SettingsEntity>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Asynchronously updates the shift settings for the specified shift identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the shift to update.</param>
    /// <param name="entity">An object containing the updated shift settings. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    Task UpdateAsync(int id, SettingsEntity entity, CancellationToken cancellationToken);
}
