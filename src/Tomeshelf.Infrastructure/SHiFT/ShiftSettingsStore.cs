using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Contracts.SHiFT;
using Tomeshelf.Application.SHiFT;
using Tomeshelf.Domain.Entities.SHiFT;
using Tomeshelf.Infrastructure.Persistence;

namespace Tomeshelf.Infrastructure.SHiFT;

public sealed class ShiftSettingsStore : IShiftSettingsStore
{
    private readonly TomeshelfShiftDbContext _db;
    private readonly IDataProtector _protector;

    public ShiftSettingsStore(TomeshelfShiftDbContext db, IDataProtectionProvider dp)
    {
        _db = db;
        _protector = dp.CreateProtector("Tomeshelf.SHiFT.ShiftSettings.Password.v1");
    }

    public async Task<ShiftSettingsDto> GetAsync(CancellationToken ct)
    {
        var row = await _db.ShiftSettings
                           .AsNoTracking()
                           .SingleOrDefaultAsync(x => x.Id == 1, ct) ??
                  new SettingsEntity();

        return new ShiftSettingsDto(row.Email, row.DefaultService, !string.IsNullOrWhiteSpace(row.EncryptedPassword), row.UpdatedUtc);
    }

    public async Task<(string Email, string Password, string Service)> GetForUseAsync(CancellationToken ct)
    {
        var row = await _db.ShiftSettings
                           .AsNoTracking()
                           .SingleOrDefaultAsync(x => x.Id == 1, ct) ??
                  throw new InvalidOperationException("SHiFT settings not configured.");

        if (string.IsNullOrWhiteSpace(row.Email))
        {
            throw new InvalidOperationException("SHiFT email not configured.");
        }

        if (string.IsNullOrWhiteSpace(row.EncryptedPassword))
        {
            throw new InvalidOperationException("SHiFT password not configured.");
        }

        var password = _protector.Unprotect(row.EncryptedPassword);

        return (row.Email, password, row.DefaultService);
    }

    public async Task UpsertAsync(ShiftSettingsUpdateRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email required.");
        }

        if (string.IsNullOrWhiteSpace(request.DefaultService))
        {
            throw new ArgumentException("DefaultService required.");
        }

        var row = await _db.ShiftSettings.SingleOrDefaultAsync(x => x.Id == 1, ct);
        if (row is null)
        {
            row = new SettingsEntity();
            _db.ShiftSettings.Add(row);
        }

        row.Email = request.Email.Trim();
        row.DefaultService = request.DefaultService.Trim();
        row.UpdatedUtc = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            row.EncryptedPassword = _protector.Protect(request.Password);
        }

        await _db.SaveChangesAsync(ct);
    }
}