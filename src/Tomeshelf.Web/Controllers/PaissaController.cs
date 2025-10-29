using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Web.Models.Paissa;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Controllers;

[Route("paissa")]
public sealed class PaissaController(IPaissaApi api) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var world = await api.GetWorldAsync(cancellationToken);

        var total = world.Districts.Sum(d => d.Tabs.Sum(t => t.Plots.Count));

        var model = new PaissaIndexViewModel
        {
                WorldId = world.WorldId,
                WorldName = world.WorldName,
                RetrievedAtUtc = world.RetrievedAtUtc,
                Districts = world.Districts,
                TotalPlotCount = total
        };

        return View(model);
    }
}