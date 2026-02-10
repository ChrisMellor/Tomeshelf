using System.Collections.Generic;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.SHiFT.Application.Features.Settings.Dtos;

namespace Tomeshelf.SHiFT.Application.Features.Settings.Queries;

public sealed record ListShiftSettingsQuery : IQuery<IReadOnlyList<ShiftSettingsDto>>;

