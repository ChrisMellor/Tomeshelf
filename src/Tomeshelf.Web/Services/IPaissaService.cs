using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Models.Paissa;

namespace Tomeshelf.Web.Services;

public interface IPaissaService
{
    Task<PaissaWorldModel> GetWorldAsync(CancellationToken cancellationToken);
}