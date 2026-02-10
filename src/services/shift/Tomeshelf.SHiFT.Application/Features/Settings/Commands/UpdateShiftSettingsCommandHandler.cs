using System;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.SHiFT.Application.Abstractions.Common;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;
using Tomeshelf.SHiFT.Application.Abstractions.Security;

namespace Tomeshelf.SHiFT.Application.Features.Settings.Commands;

public sealed class UpdateShiftSettingsCommandHandler : ICommandHandler<UpdateShiftSettingsCommand, bool>
{
    private readonly IClock _clock;
    private readonly ISecretProtector _protector;
    private readonly IShiftSettingsRepository _repository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UpdateShiftSettingsCommandHandler" /> class.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="protector">The protector.</param>
    /// <param name="clock">The clock.</param>
    public UpdateShiftSettingsCommandHandler(IShiftSettingsRepository repository, ISecretProtector protector, IClock clock)
    {
        _repository = repository;
        _protector = protector;
        _clock = clock;
    }

    /// <summary>
    ///     Handles.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public async Task<bool> Handle(UpdateShiftSettingsCommand command, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        if (await _repository.EmailExistsAsync(command.Email, command.Id, cancellationToken))
        {
            throw new InvalidOperationException("SHiFT email already exists.");
        }

        entity.Email = command.Email;
        entity.DefaultService = command.DefaultService;

        if (command.Password is not null)
        {
            entity.EncryptedPassword = command.Password.Length == 0
                ? null
                : _protector.Protect(command.Password);
        }

        entity.UpdatedUtc = _clock.UtcNow;

        await _repository.UpdateAsync(entity.Id, entity, cancellationToken);

        return true;
    }
}