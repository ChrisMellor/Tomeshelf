using System.Threading;
using System.Threading.Tasks;

namespace Tomeshelf.Application.SHiFT;

public interface IGearboxService
{
    Task<bool> RedeemCodeAsync(string shiftCode, CancellationToken ct = default);

    Task<bool> RedeemCodeAsync(string shiftCode, string? serviceOverride, CancellationToken ct = default);
}