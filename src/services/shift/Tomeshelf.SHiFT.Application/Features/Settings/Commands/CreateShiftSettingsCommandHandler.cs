using System;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.SHiFT.Application.Abstractions.Common;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;
using Tomeshelf.SHiFT.Application.Abstractions.Security;
using Tomeshelf.SHiFT.Domain.Entities;

namespace Tomeshelf.SHiFT.Application.Features.Settings.Commands;

public sealed class CreateShiftSettingsCommandHandler : ICommandHandler<CreateShiftSettingsCommand, int>
{
    private readonly IClock _clock;
    private readonly ISecretProtector _protector;
    private readonly IShiftSettingsRepository _repository;

    public CreateShiftSettingsCommandHandler(IShiftSettingsRepository repository, ISecretProtector protector, IClock clock)
    {
        _repository = repository;
        _protector = protector;
        _clock = clock;
    }

    public async Task<int> Handle(CreateShiftSettingsCommand command, CancellationToken cancellationToken)
    {
        if (await _repository.EmailExistsAsync(command.Email, null, cancellationToken))
        {
            throw new InvalidOperationException("SHiFT email already exists.");
        }

        var entity = new SettingsEntity
        {
            Email = command.Email,
            DefaultService = command.DefaultService,
            EncryptedPassword = string.IsNullOrEmpty(command.Password)
                ? null
                : _protector.Protect(command.Password),
            UpdatedUtc = _clock.UtcNow
        };

        return await _repository.CreateAsync(entity, cancellationToken);
    }
}