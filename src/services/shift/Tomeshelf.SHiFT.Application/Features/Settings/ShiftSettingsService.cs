using System;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.SHiFT.Application.Abstractions.Common;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;
using Tomeshelf.SHiFT.Application.Abstractions.Security;
using Tomeshelf.SHiFT.Application.Features.Settings.Dtos;
using Tomeshelf.SHiFT.Application.Features.Settings.UpdateSettings;
using Tomeshelf.SHiFT.Domain.Entities;

namespace Tomeshelf.SHiFT.Application.Features.Settings;

public sealed class ShiftSettingsService : IShiftSettingsService
{
    private readonly IClock _clock;
    private readonly ISecretProtector _protector;
    private readonly IShiftSettingsRepository _repository;

    public ShiftSettingsService(IShiftSettingsRepository repository, ISecretProtector protector, IClock clock)
    {
        _repository = repository;
        _protector = protector;
        _clock = clock;
    }

    public async Task<int> CreateAsync(CreateShiftSettingsCommand cmd, CancellationToken ct)
    {
        if (await _repository.EmailExistsAsync(cmd.Email, null, ct))
        {
            throw new InvalidOperationException("SHiFT email already exists.");
        }

        var entity = new SettingsEntity
        {
            Email = cmd.Email,
            DefaultService = cmd.DefaultService,
            EncryptedPassword = string.IsNullOrEmpty(cmd.Password)
                ? null
                : _protector.Protect(cmd.Password),
            UpdatedUtc = _clock.UtcNow
        };

        return await _repository.CreateAsync(entity, ct);
    }

    public Task DeleteAsync(int id, CancellationToken ct)
    {
        return _repository.DeleteAsync(id, ct);
    }

    public async Task<ShiftSettingsDto?> GetAsync(int id, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
        {
            return null;
        }

        return new ShiftSettingsDto(entity.Id, entity.Email, entity.DefaultService, !string.IsNullOrWhiteSpace(entity.EncryptedPassword), entity.UpdatedUtc);
    }

    public async Task<bool> UpdateAsync(UpdateShiftSettingsCommand cmd, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(cmd.Id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        if (await _repository.EmailExistsAsync(cmd.Email, cmd.Id, cancellationToken))
        {
            throw new InvalidOperationException("SHiFT email already exists.");
        }

        entity.Email = cmd.Email;
        entity.DefaultService = cmd.DefaultService;

        if (cmd.Password is not null)
        {
            entity.EncryptedPassword = cmd.Password.Length == 0
                ? null
                : _protector.Protect(cmd.Password);
        }

        entity.UpdatedUtc = _clock.UtcNow;

        await _repository.UpdateAsync(entity.Id, entity, cancellationToken);

        return true;
    }
}

