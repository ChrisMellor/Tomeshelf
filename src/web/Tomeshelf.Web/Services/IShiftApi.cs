using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Models.Shift;

namespace Tomeshelf.Web.Services;

public interface IShiftApi
{
    Task<RedeemResponseModel> RedeemCodeAsync(string code, CancellationToken cancellationToken);

    Task<IReadOnlyList<ShiftAccountModel>> GetAccountsAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Creates a new SHiFT account configuration entry.
    /// </summary>
    /// <returns><see langword="true" /> if the account was created; <see langword="false" /> if it already exists.</returns>
    Task<bool> CreateAccountAsync(ShiftAccountEditorModel model, CancellationToken cancellationToken);

    Task DeleteAccountAsync(int id, CancellationToken cancellationToken);
}
