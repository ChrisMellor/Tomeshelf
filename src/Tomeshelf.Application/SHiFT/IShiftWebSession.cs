using System.Threading;
using System.Threading.Tasks;

namespace Tomeshelf.Application.SHiFT;

public interface IShiftWebSession
{
    Task<string> BuildRedeemBodyAsync(string code, string csrfToken, string service, CancellationToken ct = default);

    ValueTask DisposeAsync();

    Task<string> GetCsrfFromHomeAsync(CancellationToken ct = default);

    Task<string> GetCsrfFromRewardsAsync(CancellationToken ct = default);

    Task LoginAsync(string email, string password, string csrfToken, CancellationToken ct = default);

    Task RedeemAsync(string redeemBody, CancellationToken ct = default

}