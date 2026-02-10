using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.SHiFT.Application.Abstractions.Persistence;
using Tomeshelf.SHiFT.Application.Features.Settings.Dtos;

namespace Tomeshelf.SHiFT.Application.Features.Settings.Queries;

public sealed class GetShiftSettingsQueryHandler : IQueryHandler<GetShiftSettingsQuery, ShiftSettingsDto?>
{
    private readonly IShiftSettingsRepository _repository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetShiftSettingsQueryHandler" /> class.
    /// </summary>
    /// <param name="repository">The repository.</param>
    public GetShiftSettingsQueryHandler(IShiftSettingsRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    ///     Handles.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public async Task<ShiftSettingsDto?> Handle(GetShiftSettingsQuery query, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(query.Id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        return new ShiftSettingsDto(entity.Id, entity.Email, entity.DefaultService, !string.IsNullOrWhiteSpace(entity.EncryptedPassword), entity.UpdatedUtc);
    }
}