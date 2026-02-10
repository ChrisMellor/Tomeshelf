using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Models.Paissa;

namespace Tomeshelf.Web.Services;

public interface IPaissaApi
{
    /// <summary>
    ///     Gets the world asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    Task<PaissaWorldModel> GetWorldAsync(CancellationToken cancellationToken);
}