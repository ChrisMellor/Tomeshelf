using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;

namespace Tomeshelf.SHiFT.Application.Features.Settings.Commands;

public sealed class DeleteShiftSettingsCommandHandler : ICommandHandler<DeleteShiftSettingsCommand, bool>
{
    private readonly IShiftSettingsRepository _repository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeleteShiftSettingsCommandHandler" /> class.
    /// </summary>
    /// <param name="repository">The repository.</param>
    public DeleteShiftSettingsCommandHandler(IShiftSettingsRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    ///     Handles.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public async Task<bool> Handle(DeleteShiftSettingsCommand command, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(command.Id, cancellationToken);

        return true;
    }
}