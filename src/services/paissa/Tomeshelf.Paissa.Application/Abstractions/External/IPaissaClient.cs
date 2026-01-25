using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Paissa.Domain.Entities;

namespace Tomeshelf.Paissa.Application.Abstractions.External;

/// <summary>
///     Defines a contract for retrieving world information from the Paissa service asynchronously.
/// </summary>
/// <remarks>
///     Implementations of this interface provide access to world data by world ID, supporting cancellation
///     through a token. This interface is intended for use in applications that require integration with the Paissa
///     service
///     to obtain world-specific details.
/// </remarks>
public interface IPaissaClient
{
    Task<PaissaWorld> GetWorldAsync(int worldId, CancellationToken cancellationToken);
}
