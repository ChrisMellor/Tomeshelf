using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Models.Paissa;

namespace Tomeshelf.Web.Services;

public interface IPaissaApi
{
    Task<PaissaWorldModel> GetWorldAsync(CancellationToken cancellationToken);
}