using System;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.SHiFT;

public sealed class GearboxService : IGearboxService
{
    private readonly IShiftWebSession _session;
    private readonly IShiftSettingsStore _settings;

    public GearboxService(IShiftSettingsStore settings, IShiftWebSession session)
    {
        _settings = settings;
        _session = session;
    }

    public Task<bool> RedeemCodeAsync(string shiftCode, CancellationToken ct = default)
    {
        return RedeemCodeAsync(shiftCode, null, ct);
    }

    public async Task<bool> RedeemCodeAsync(string shiftCode, string? serviceOverride, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(shiftCode))
        {
            throw new ArgumentException("SHiFT code is required.", nameof(shiftCode));
        }

        var (email, password, defaultService) = await _settings.GetForUseAsync(ct);
        var service = string.IsNullOrWhiteSpace(serviceOverride)
            ? defaultService
            : serviceOverride.Trim();

        var csrfHome = await _session.GetCsrfFromHomeAsync(ct);
        await _session.LoginAsync(email, password, csrfHome, ct);

        var csrfRewards = await _session.GetCsrfFromRewardsAsync(ct);

        var redeemBody = await _session.BuildRedeemBodyAsync(shiftCode.Trim(), csrfRewards, service, ct);
        await _session.RedeemAsync(redeemBody, ct);

        return true;
    }
}