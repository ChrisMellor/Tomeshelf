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
    private readonly IPaissaClient _client;
    private readonly ILogger<PaissaHousingService> _logger;
    private readonly int _worldId;

    public PaissaHousingService(IPaissaClient client, IConfiguration configuration, ILogger<PaissaHousingService> logger)
    {
        _client = client;
        _logger = logger;
        _worldId = configuration.GetValue("Paissa:WorldId", 33);
    }

    public async Task<PaissaWorldResponse> GetAcceptingEntriesAsync(CancellationToken cancellationToken)
    {
        var world = await _client.GetWorldAsync(_worldId, cancellationToken);
        var retrievedAt = DateTimeOffset.UtcNow;

        var districtResponses = new List<PaissaDistrictResponse>();

        foreach (var district in world.Districts)
        {
            var groupedPlots = district.OpenPlots
                                       .Where(plot => plot.LotteryPhase == (int)LotteryPhase.AcceptingEntries)
                                       .Select(plot => (Plot: plot, Category: MapSize(plot.Size)))
                                       .GroupBy(x => x.Category.Key, StringComparer.OrdinalIgnoreCase)
                                       .Select(group =>
                                        {
                                            var orderedPlots = group.Select(item => MapPlot(item.Plot))
                                                                    .OrderBy(p => p.Ward)
                                                                    .ThenBy(p => p.Plot)
                                                                    .ToList()
                                                                    .AsReadOnly();

                                            return new PaissaSizeGroupResponse(group.First().Category.Label, group.Key, orderedPlots);
                                        })
                                       .OrderBy(group => group.SizeKey, SizeKeyComparer.Instance)
                                       .ToList();

            if (groupedPlots.Count == 0)
            {
                continue;
            }

            districtResponses.Add(new PaissaDistrictResponse(district.Id, district.Name, groupedPlots.AsReadOnly()));
        }

        var orderedDistricts = districtResponses.OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                                                .ToList()
                                                .AsReadOnly();

        return new PaissaWorldResponse(world.Id, world.Name, retrievedAt, orderedDistricts);
    }

    private static PaissaPlotResponse MapPlot(PaissaPlotDto plot)
    {
        var lastUpdated = ConvertUnixTimestamp(plot.LastUpdatedTime);
        var entries = plot.LotteryEntries.GetValueOrDefault();
        var purchase = MapPurchaseCategory(plot.PurchaseSystem);

        return new PaissaPlotResponse(plot.WardNumber + 1, plot.PlotNumber + 1, plot.Price, entries, lastUpdated, purchase.Display, purchase.Key);
    }

    private static (string Label, string Key) MapSize(int rawSize)
    {
        return rawSize switch
        {
            0 => ("Small", "small"),
            1 => ("Medium", "medium"),
            2 => ("Large", "large"),
            _ => ("Unknown", "unknown")
        };
    }

    private static (string Display, string Key) MapPurchaseCategory(int purchaseSystem)
    {
        var allowsFreeCompany = (purchaseSystem & (int)PurchaseSystem.FreeCompany) != 0;
        var allowsPersonal = (purchaseSystem & (int)PurchaseSystem.Personal) != 0;

        if (allowsFreeCompany && allowsPersonal)
        {
            return ("Personal & Free Company", "both");
        }

        if (allowsPersonal)
        {
            return ("Personal", "personal");
        }

        if (allowsFreeCompany)
        {
            return ("Free Company", "free-company");
        }

        return ("Unknown", "unknown");
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

    private sealed class SizeKeyComparer : IComparer<string>
    {
        public static SizeKeyComparer Instance { get; } = new();

        public int Compare(string? x, string? y)
        {
            var rankX = Rank(x);
            var rankY = Rank(y);
            return rankX.CompareTo(rankY);
        }

        private static int Rank(string? value)
        {
            return value?.ToLowerInvariant() switch
            {
                "small" => 0,
                "medium" => 1,
                "large" => 2,
                _ => 3
            };
        }
    }
}
