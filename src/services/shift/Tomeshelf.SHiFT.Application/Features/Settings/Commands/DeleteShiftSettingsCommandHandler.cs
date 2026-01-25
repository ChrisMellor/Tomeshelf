using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.SHiFT.Application.Abstractions.Messaging;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;

namespace Tomeshelf.SHiFT.Application.Features.Settings.Commands;

public sealed class DeleteShiftSettingsCommandHandler : ICommandHandler<DeleteShiftSettingsCommand, bool>
{
    private readonly IShiftSettingsRepository _repository;

    public DeleteShiftSettingsCommandHandler(IShiftSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteShiftSettingsCommand command, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(command.Id, cancellationToken);

        return true;
    }
}
