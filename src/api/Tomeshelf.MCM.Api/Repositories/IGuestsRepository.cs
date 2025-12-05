using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.MCM.Api.Enums;
using Tomeshelf.MCM.Api.Records;

namespace Tomeshelf.MCM.Api.Repositories;

/// <summary>
///     Defines methods for managing guest data within a specific city, including retrieval, deletion, and synchronization
///     operations.
/// </summary>
/// <remarks>
///     Implementations of this interface should provide asynchronous access to guest records, supporting
///     paging and bulk operations. Methods accept a cancellation token to allow callers to cancel long-running operations.
///     This interface is intended for use in scenarios where guest information must be queried or updated in a
///     city-specific context.
/// </remarks>
public interface IGuestsRepository
{
    /// <summary>
    ///     Asynchronously deletes all records associated with the specified city.
    /// </summary>
    /// <param name="city">The city for which all related records will be deleted. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of records deleted.</returns>
    Task<int> DeleteAllAsync(City city, CancellationToken cancellationToken);

    /// <summary>
    ///     Asynchronously retrieves a paginated snapshot of guests for the specified city.
    /// </summary>
    /// <param name="city">The city for which to retrieve guest information. Cannot be null.</param>
    /// <param name="page">The zero-based index of the page to retrieve. Must be greater than or equal to 0.</param>
    /// <param name="pageSize">The maximum number of guests to include in the page. Must be greater than 0.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a <see cref="GuestSnapshot" /> with
    ///     guest data for the specified page. If no guests are found, the snapshot will contain an empty collection.
    /// </returns>
    Task<GuestSnapshot> GetPageAsync(City city, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    ///     Creates or updates a snapshot for the specified city using the provided guest records, and returns the resulting
    ///     synchronization delta.
    /// </summary>
    /// <param name="city">The city for which the snapshot is to be upserted. Cannot be null.</param>
    /// <param name="guests">
    ///     A read-only list of guest records to include in the snapshot. Cannot be null or contain null
    ///     elements.
    /// </param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a SyncDelta object describing the
    ///     changes made during the upsert.
    /// </returns>
    Task<SyncDelta> UpsertSnapshotAsync(City city, IReadOnlyList<McmGuestRecord> guests, CancellationToken cancellationToken);
}