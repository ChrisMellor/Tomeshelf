using Tomeshelf.SHiFT.Application.Abstractions.Messaging;
using Tomeshelf.SHiFT.Application.Features.Settings.Dtos;

namespace Tomeshelf.SHiFT.Application.Features.Settings.Queries;

public sealed record GetShiftSettingsQuery(int Id) : IQuery<ShiftSettingsDto?>;
