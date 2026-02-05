using System.Collections.Generic;

namespace Tomeshelf.Fitbit.Application.Features.Overview.Models;

public sealed record FitbitOverviewRangeDto
{
    public required int Days { get; init; }

    public required IReadOnlyList<FitbitOverviewDayDto> Items { get; init; }
}