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

    /// <summary>
    ///     Initializes a new instance of the <see cref="CreateShiftSettingsCommandHandler" /> class.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="protector">The protector.</param>
    /// <param name="clock">The clock.</param>
    public CreateShiftSettingsCommandHandler(IShiftSettingsRepository repository, ISecretProtector protector, IClock clock)
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