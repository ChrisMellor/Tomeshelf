using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.SHiFT.Application.Features.Settings.Dtos;
using Tomeshelf.SHiFT.Application.Features.Settings.UpdateSettings;

namespace Tomeshelf.SHiFT.Application.Features.Settings;

public interface IShiftSettingsService
{
    Task<int> CreateAsync(CreateShiftSettingsCommand cmd, CancellationToken ct);

    Task DeleteAsync(int id, CancellationToken ct);

    Task<ShiftSettingsDto?> GetAsync(int id, CancellationToken ct);

    Task<bool> UpdateAsync(UpdateShiftSettingsCommand cmd, CancellationToken cancellationToken);
}
