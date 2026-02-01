using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.SHiFT.Application.Abstractions.Common;
using Tomeshelf.SHiFT.Application.Abstractions.External;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Models;

namespace Tomeshelf.SHiFT.Application.Features.KeyDiscovery.Commands;

public sealed class SweepShiftKeysCommandHandler : ICommandHandler<SweepShiftKeysCommand, ShiftKeySweepResult>
{
    private static readonly TimeSpan MinimumLookback = TimeSpan.FromHours(1);
    private static readonly TimeSpan MaximumLookback = TimeSpan.FromDays(7);

    private readonly IClock _clock;
    private readonly IGearboxClient _gearboxClient;
    private readonly IEnumerable<IShiftKeySource> _sources;

    public SweepShiftKeysCommandHandler(
        IEnumerable<IShiftKeySource> sources,
        IGearboxClient gearboxClient,
        IClock clock)
    {
        _sources = sources;
        _gearboxClient = gearboxClient;
        _clock = clock;
    }

    public async Task<ShiftKeySweepResult> Handle(SweepShiftKeysCommand command, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var lookback = Clamp(command.Lookback);
        var sinceUtc = now - lookback;

        var candidates = new List<ShiftKeyCandidate>();
        foreach (var source in _sources)
        {
            var found = await source.GetKeysAsync(sinceUtc, cancellationToken);
            if (found?.Count > 0)
            {
                candidates.AddRange(found);
            }
        }

        var items = new List<ShiftKeySweepItem>();
        var grouped = candidates
            .Where(c => !string.IsNullOrWhiteSpace(c.Code))
            .GroupBy(c => c.Code.Trim(), StringComparer.OrdinalIgnoreCase);

        foreach (var group in grouped)
        {
            var code = group.Key.ToUpperInvariant();
            var sources = group.Select(c => c.Source)
                .Where(source => !string.IsNullOrWhiteSpace(source))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(source => source, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var results = await _gearboxClient.RedeemCodeAsync(code, cancellationToken);

            items.Add(new ShiftKeySweepItem(code, sources, results));
        }

        var totalAttempts = items.Sum(item => item.Results.Count);
        var succeeded = items.Sum(item => item.Results.Count(result => result.Success));
        var failed = totalAttempts - succeeded;

        var summary = new ShiftKeySweepSummary(items.Count, totalAttempts, succeeded, failed);

        return new ShiftKeySweepResult(sinceUtc, now, summary, items);
    }

    private static TimeSpan Clamp(TimeSpan value)
    {
        if (value < MinimumLookback)
        {
            return MinimumLookback;
        }

        if (value > MaximumLookback)
        {
            return MaximumLookback;
        }

        return value;
    }
}
