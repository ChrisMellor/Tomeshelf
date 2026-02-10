using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Paissa.Application.Abstractions.Common;
using Tomeshelf.Paissa.Application.Abstractions.External;
using Tomeshelf.Paissa.Application.Features.Housing.Dtos;
using Tomeshelf.Paissa.Domain.Entities;
using Tomeshelf.Paissa.Domain.ValueObjects;

namespace Tomeshelf.Paissa.Application.Features.Housing.Queries;

public sealed class GetAcceptingEntriesQueryHandler : IQueryHandler<GetAcceptingEntriesQuery, PaissaWorldSummaryDto>
{
    private static readonly (HousingPlotSize Size, string Key, string Label)[] SizeDefinitions = { (HousingPlotSize.Large, "large", "Large"), (HousingPlotSize.Medium, "medium", "Medium"), (HousingPlotSize.Small, "small", "Small") };

    private readonly IPaissaClient _client;
    private readonly IClock _clock;
    private readonly IPaissaWorldSettings _settings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetAcceptingEntriesQueryHandler" /> class.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="settings">The settings.</param>
    /// <param name="clock">The clock.</param>
    public GetAcceptingEntriesQueryHandler(IPaissaClient client, IPaissaWorldSettings settings, IClock clock)
    {
        _client = client;
        _settings = settings;
        _clock = clock;
    }

    /// <summary>
    ///     Handles.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    public async Task<PaissaWorldSummaryDto> Handle(GetAcceptingEntriesQuery query, CancellationToken cancellationToken)
    {
        var world = await _client.GetWorldAsync(_settings.WorldId, cancellationToken);
        var acceptingWorld = world.FilterAcceptingEntryDistricts(true);
        var retrievedAtUtc = _clock.UtcNow;
        var districts = new List<PaissaDistrictSummaryDto>();

        foreach (var district in acceptingWorld.Districts)
        {
            var plotsBySize = district.OpenPlots
                                      .GroupBy(plot => plot.Size)
                                      .ToDictionary(group => group.Key, group => group.Select(MapPlot)
                                                                                      .OrderBy(plot => plot.Ward)
                                                                                      .ThenBy(plot => plot.Plot)
                                                                                      .ToList());

            if (plotsBySize.Count == 0)
            {
                continue;
            }

            var sizeGroups = SizeDefinitions.Select(definition => plotsBySize.TryGetValue(definition.Size, out var plots)
                                                        ? new PaissaSizeGroupSummaryDto(definition.Label, definition.Key, plots)
                                                        : new PaissaSizeGroupSummaryDto(definition.Label, definition.Key, Array.Empty<PaissaPlotSummaryDto>()))
                                            .ToList();

            districts.Add(new PaissaDistrictSummaryDto(district.Id, district.Name, sizeGroups));
        }

        var orderedDistricts = districts.OrderBy(district => district.Name, StringComparer.OrdinalIgnoreCase)
                                        .ToList();

        return new PaissaWorldSummaryDto(world.Id, world.Name, retrievedAtUtc, orderedDistricts);
    }

    /// <summary>
    ///     Maps the plot.
    /// </summary>
    /// <param name="plot">The plot.</param>
    /// <returns>The result of the operation.</returns>
    private static PaissaPlotSummaryDto MapPlot(PaissaPlot plot)
    {
        var entries = plot.LotteryEntries.GetValueOrDefault();
        var eligibility = plot.Eligibility;

        return new PaissaPlotSummaryDto(plot.WardNumber, plot.PlotNumber, plot.Price, entries, plot.LastUpdatedUtc, eligibility.AllowsPersonal, eligibility.AllowsFreeCompany, eligibility.IsUnknown);
    }
}