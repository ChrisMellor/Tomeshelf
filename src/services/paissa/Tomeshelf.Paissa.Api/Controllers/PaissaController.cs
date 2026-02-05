using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Paissa.Api.Contracts;
using Tomeshelf.Paissa.Application.Features.Housing.Dtos;
using Tomeshelf.Paissa.Application.Features.Housing.Queries;

namespace Tomeshelf.Paissa.Api.Controllers;

[ApiController]
[Route("paissa")]
public sealed class PaissaController(IQueryHandler<GetAcceptingEntriesQuery, PaissaWorldSummaryDto> handler) : ControllerBase
{
    /// <summary>
    ///     Returns the housing plots currently accepting entries for the configured world.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    [HttpGet("world")]
    [ProducesResponseType(typeof(PaissaWorldResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaissaWorldResponse>> GetWorld(CancellationToken cancellationToken)
    {
        var world = await handler.Handle(new GetAcceptingEntriesQuery(), cancellationToken);
        var response = MapWorld(world);

        return Ok(response);
    }

    private static PaissaDistrictResponse MapDistrict(PaissaDistrictSummaryDto district)
    {
        var tabs = district.SizeGroups
                           .Select(group => new PaissaSizeGroupResponse(group.Size, group.SizeKey, group.Plots
                                                                                                        .Select(MapPlot)
                                                                                                        .ToList()))
                           .ToList();

        return new PaissaDistrictResponse(district.Id, district.Name, tabs);
    }

    private static PaissaPlotResponse MapPlot(PaissaPlotSummaryDto plot)
    {
        return new PaissaPlotResponse(plot.Ward, plot.Plot, plot.Price, plot.Entries, plot.LastUpdatedUtc, plot.AllowsPersonal, plot.AllowsFreeCompany, plot.IsEligibilityUnknown);
    }

    private static PaissaWorldResponse MapWorld(PaissaWorldSummaryDto world)
    {
        var districts = world.Districts
                             .Select(MapDistrict)
                             .ToList();

        return new PaissaWorldResponse(world.WorldId, world.WorldName, world.RetrievedAtUtc, districts);
    }
}