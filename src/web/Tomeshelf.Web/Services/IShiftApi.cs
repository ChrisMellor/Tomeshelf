using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Web.Models.Shift;

namespace Tomeshelf.Web.Services;

public interface IShiftApi
{
    Task<RedeemResponseModel> RedeemCodeAsync(string code, CancellationToken cancellationToken);
}
