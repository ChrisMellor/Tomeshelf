using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Models.Shift;

namespace Tomeshelf.Web.Services;

public interface IShiftApi
{
    /// <summary>
    ///     Redeems the code asynchronously.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    Task<RedeemResponseModel> RedeemCodeAsync(string code, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the accounts asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    Task<IReadOnlyList<ShiftAccountModel>> GetAccountsAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Creates a new SHiFT account configuration entry.
    /// </summary>
    /// <returns><see langword="true" /> if the account was created; <see langword="false" /> if it already exists.</returns>
    Task<bool> CreateAccountAsync(ShiftAccountEditorModel model, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes the account asynchronously.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteAccountAsync(int id, CancellationToken cancellationToken);
}
