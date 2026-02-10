using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;
using Tomeshelf.SHiFT.Application.Features.Settings.Dtos;

namespace Tomeshelf.SHiFT.Application.Features.Settings.Queries;

public sealed class ListShiftSettingsQueryHandler : IQueryHandler<ListShiftSettingsQuery, IReadOnlyList<ShiftSettingsDto>>
{
    private readonly IShiftSettingsRepository _repository;

    public ListShiftSettingsQueryHandler(IShiftSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ShiftSettingsDto>> Handle(ListShiftSettingsQuery query, CancellationToken cancellationToken)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);

        return entities.Select(entity => new ShiftSettingsDto(entity.Id, entity.Email, entity.DefaultService, !string.IsNullOrWhiteSpace(entity.EncryptedPassword), entity.UpdatedUtc))
                       .ToList();
    }
}

