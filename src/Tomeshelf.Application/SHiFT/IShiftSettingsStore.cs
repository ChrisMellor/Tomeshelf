using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.SHiFT.Api;

namespace Tomeshelf.Application.SHiFT;

public interface IShiftSettingsStore
{
    Task<ShiftSettingsDto> GetAsync(CancellationToken ct);

    Task<(string Email, string Password, string Service)> GetForUseAsync(CancellationToken ct);

    Task UpsertAsync(ShiftSettingsUpdateRequest request, CancellationToken ct);
}