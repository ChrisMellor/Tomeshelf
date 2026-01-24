using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Paissa.Api.Models;

namespace Tomeshelf.Paissa.Application;

public interface IPaissaClient
{
    Task<PaissaWorldDto> GetWorldAsync(int worldId, CancellationToken cancellationToken);
}