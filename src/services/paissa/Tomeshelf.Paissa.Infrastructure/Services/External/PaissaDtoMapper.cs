using System;
using System.Linq;
using Tomeshelf.Paissa.Domain.Entities;
using Tomeshelf.Paissa.Domain.ValueObjects;

namespace Tomeshelf.Paissa.Infrastructure.Services.External;

/// <summary>
///     Provides static methods for mapping Paissa-related data transfer objects (DTOs) to their corresponding domain model
///     types.
/// </summary>
/// <remarks>
///     Use this class to convert PaissaWorldDto, PaissaDistrictDto, and PaissaPlotDto instances into domain
///     models for use within the application. The mapping methods handle nested collections and apply necessary data
///     transformations to ensure the resulting domain objects are correctly populated.
/// </remarks>
internal static class PaissaDtoMapper
{
    /// <summary>
    ///     Maps a PaissaWorldDto instance to a corresponding PaissaWorld object, including its districts.
    /// </summary>
    /// <remarks>
    ///     If the Districts property of the DTO is null, an empty list of districts is used in the
    ///     resulting PaissaWorld.
    /// </remarks>
    /// <param name="dto">The data transfer object containing world information to map. Cannot be null.</param>
    /// <returns>
    ///     A PaissaWorld object populated with data from the specified DTO. If the DTO contains no districts, the resulting
    ///     world will have an empty district list.
    /// </returns>
    public static PaissaWorld MapWorld(PaissaWorldDto dto)
    {
        var districts = (dto.Districts ?? Array.Empty<PaissaDistrictDto>()).Select(MapDistrict)
                                                                           .ToList();

        return PaissaWorld.Create(dto.Id, dto.Name, districts);
    }

    /// <summary>
    ///     Converts a Unix time value, expressed as a number of seconds since the Unix epoch, to an equivalent
    ///     DateTimeOffset.
    /// </summary>
    /// <remarks>
    ///     This method supports both whole and fractional seconds. If the fractional part is negligible,
    ///     the result will correspond to the exact epoch second; otherwise, the fractional seconds are included in the
    ///     returned value.
    /// </remarks>
    /// <param name="value">
    ///     The Unix time value, represented as a double, indicating the number of seconds (including fractional seconds)
    ///     that have elapsed since January 1, 1970, 00:00:00 UTC.
    /// </param>
    /// <returns>A DateTimeOffset that represents the date and time corresponding to the specified Unix time value.</returns>
    private static DateTimeOffset FromUnixTime(double value)
    {
        var seconds = Math.Truncate(value);
        var fractional = value - seconds;
        var epoch = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(seconds));

        return fractional <= double.Epsilon
            ? epoch
            : epoch.AddSeconds(fractional);
    }

    /// <summary>
    ///     Maps a PaissaDistrictDto instance to a corresponding PaissaDistrict domain model.
    /// </summary>
    /// <remarks>
    ///     If the OpenPlots property of the DTO is null, an empty list of plots is used for the
    ///     resulting district.
    /// </remarks>
    /// <param name="dto">
    ///     The data transfer object containing the district's identifier, name, and associated plots. Cannot be
    ///     null.
    /// </param>
    /// <returns>A PaissaDistrict instance populated with values from the specified DTO, including a list of mapped plots.</returns>
    private static PaissaDistrict MapDistrict(PaissaDistrictDto dto)
    {
        var plots = (dto.OpenPlots ?? Array.Empty<PaissaPlotDto>()).Select(MapPlot)
                                                                   .ToList();

        return PaissaDistrict.Create(dto.Id, dto.Name, plots);
    }

    /// <summary>
    ///     Maps the specified raw phase integer to its corresponding LotteryPhase enumeration value.
    /// </summary>
    /// <remarks>
    ///     Use this method to convert integer-based phase representations, such as those received from
    ///     external sources, into strongly typed LotteryPhase values for safer and more readable code.
    /// </remarks>
    /// <param name="rawPhase">
    ///     The raw phase integer representing the current lottery phase. Valid values are 1 for accepting entries, 2 for
    ///     results processing, and 3 for winners announced. If the value is null or outside this range, the method returns
    ///     LotteryPhase.Unknown.
    /// </param>
    /// <returns>
    ///     The corresponding LotteryPhase enumeration value based on the provided raw phase integer. Returns
    ///     LotteryPhase.Unknown if the input is null or not a recognized phase.
    /// </returns>
    private static LotteryPhase MapLotteryPhase(int? rawPhase)
    {
        return rawPhase switch
        {
            1 => LotteryPhase.AcceptingEntries,
            2 => LotteryPhase.ResultsProcessing,
            3 => LotteryPhase.WinnersAnnounced,
            _ => LotteryPhase.Unknown
        };
    }

    /// <summary>
    ///     Maps a PaissaPlotDto instance to a new PaissaPlot object, converting and assigning relevant property values.
    /// </summary>
    /// <remarks>
    ///     The method adjusts the ward and plot numbers to be one-based, converts the last updated time
    ///     from Unix time, and maps size, purchase system, and lottery phase values as needed.
    /// </remarks>
    /// <param name="dto">The data transfer object containing plot information to be mapped. Cannot be null.</param>
    /// <returns>A PaissaPlot object populated with values mapped from the specified dto.</returns>
    private static PaissaPlot MapPlot(PaissaPlotDto dto)
    {
        var size = MapSize(dto.Size);
        var phase = MapLotteryPhase(dto.LotteryPhase);
        var purchaseSystem = MapPurchaseSystem(dto.PurchaseSystem);
        var lastUpdated = FromUnixTime(dto.LastUpdatedTime);

        return PaissaPlot.Create(dto.WardNumber + 1, dto.PlotNumber + 1, size, dto.Price, lastUpdated, purchaseSystem, dto.LotteryEntries, phase);
    }

    /// <summary>
    ///     Maps a raw integer value to its corresponding PurchaseSystem enumeration value.
    /// </summary>
    /// <remarks>
    ///     This method interprets the rawSystem parameter as a set of bitwise flags and returns a
    ///     combination of PurchaseSystem values that are enabled. If no recognized flags are set, PurchaseSystem.None is
    ///     returned.
    /// </remarks>
    /// <param name="rawSystem">
    ///     An integer representing one or more purchase system options, where each option is encoded as a
    ///     bit flag.
    /// </param>
    /// <returns>
    ///     A PurchaseSystem value that reflects the purchase systems indicated by the specified bitwise flags in
    ///     rawSystem.
    /// </returns>
    private static PurchaseSystem MapPurchaseSystem(int rawSystem)
    {
        var system = PurchaseSystem.None;

        if ((rawSystem & (int)PurchaseSystem.FreeCompany) != 0)
        {
            system |= PurchaseSystem.FreeCompany;
        }

        if ((rawSystem & (int)PurchaseSystem.Personal) != 0)
        {
            system |= PurchaseSystem.Personal;
        }

        return system;
    }

    /// <summary>
    ///     Maps an integer value to its corresponding housing plot size enumeration.
    /// </summary>
    /// <param name="rawSize">
    ///     An integer representing the raw size of the housing plot. Valid values are 0 for small, 1 for medium, and 2 for
    ///     large. Any other value is treated as unknown.
    /// </param>
    /// <returns>
    ///     A value of the HousingPlotSize enumeration that corresponds to the specified raw size. Returns
    ///     HousingPlotSize.Unknown if the input does not match a known size.
    /// </returns>
    private static HousingPlotSize MapSize(int rawSize)
    {
        return rawSize switch
        {
            0 => HousingPlotSize.Small,
            1 => HousingPlotSize.Medium,
            2 => HousingPlotSize.Large,
            _ => HousingPlotSize.Unknown
        };
    }
}