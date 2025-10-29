using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tomeshelf.Paissa.Api.Models;

#nullable enable

namespace Tomeshelf.Paissa.Api.Services;

public sealed class PaissaHousingService
{
    private static readonly (string Key, string Label)[] SizeDefinitions =
    {
        ("large", "Large"),
        ("medium", "Medium"),
        ("small", "Small")
    };

    private readonly IPaissaClient _client;
    private readonly int _worldId;

    public PaissaHousingService(IPaissaClient client, IConfiguration configuration, ILogger<PaissaHousingService> logger)
    {
        _client = client;
        _worldId = configuration.GetValue("Paissa:WorldId", 33);
        _ = logger;
    }

    public async Task<PaissaWorldResponse> GetAcceptingEntriesAsync(CancellationToken cancellationToken)
    {
        var world = await _client.GetWorldAsync(_worldId, cancellationToken);
        var retrievedAt = DateTimeOffset.UtcNow;

        var districtResponses = new List<PaissaDistrictResponse>();

        foreach (var district in world.Districts)
        {
            var plotsBySize = district.OpenPlots
                                      .Select(plot => new { Plot = plot, SizeKey = MapSizeKey(plot.Size) })
                                      .Where(x => x.SizeKey is not null && x.Plot.LotteryPhase == (int)LotteryPhase.AcceptingEntries)
                                      .GroupBy(x => x.SizeKey!, StringComparer.OrdinalIgnoreCase)
                                      .ToDictionary(
                                          group => group.Key,
                                          group => group.Select(x => MapPlot(x.Plot))
                                                        .OrderBy(p => p.Ward)
                                                        .ThenBy(p => p.Plot)
                                                        .ToList(),
                                          StringComparer.OrdinalIgnoreCase);

            if (!plotsBySize.Any())
            {
                continue;
            }

            var sizeGroups = SizeDefinitions.Select(definition =>
                                      {
                                          if (plotsBySize.TryGetValue(definition.Key, out var plots))
                                          {
                                              return new PaissaSizeGroupResponse(definition.Label, definition.Key, plots);
                                          }

                                          return new PaissaSizeGroupResponse(definition.Label, definition.Key, Array.Empty<PaissaPlotResponse>());
                                      })
                                      .ToList();

            districtResponses.Add(new PaissaDistrictResponse(district.Id, district.Name, sizeGroups));
        }

        var orderedDistricts = districtResponses.OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                                                .ToList();

        return new PaissaWorldResponse(world.Id, world.Name, retrievedAt, orderedDistricts);
    }

    private static PaissaPlotResponse MapPlot(PaissaPlotDto plot)
    {
        var lastUpdated = ConvertUnixTimestamp(plot.LastUpdatedTime);
        var entries = plot.LotteryEntries.GetValueOrDefault();
        var eligibility = MapPurchaseCategory(plot.PurchaseSystem);

        return new PaissaPlotResponse(plot.WardNumber + 1, plot.PlotNumber + 1, plot.Price, entries, lastUpdated, eligibility.AllowsPersonal, eligibility.AllowsFreeCompany, eligibility.IsUnknown);
    }

    private static string? MapSizeKey(int rawSize)
    {
        return rawSize switch
        {
            2 => "large",
            1 => "medium",
            0 => "small",
            _ => null
        };
    }

    private static (bool AllowsPersonal, bool AllowsFreeCompany, bool IsUnknown) MapPurchaseCategory(int purchaseSystem)
    {
        var allowsFreeCompany = (purchaseSystem & (int)PurchaseSystem.FreeCompany) != 0;
        var allowsPersonal = (purchaseSystem & (int)PurchaseSystem.Personal) != 0;

        if (!allowsFreeCompany && !allowsPersonal)
        {
            return (false, false, true);
        }

        return (allowsPersonal, allowsFreeCompany, false);
    }

    private static DateTimeOffset ConvertUnixTimestamp(double value)
    {
        var seconds = Math.Truncate(value);
        var fractional = value - seconds;
        var epoch = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(seconds));

        if (fractional <= double.Epsilon)
        {
            return epoch;
        }

        return epoch.AddSeconds(fractional);
    }

    private enum LotteryPhase
    {
        AcceptingEntries = 1,
        ResultsProcessing = 2,
        WinnersAnnounced = 3
    }

    [Flags]
    private enum PurchaseSystem
    {
        Unknown = 1,
        FreeCompany = 2,
        Personal = 4
    }
}
