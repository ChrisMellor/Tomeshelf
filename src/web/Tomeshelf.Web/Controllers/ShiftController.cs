using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Web.Models.Shift;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Controllers;

[Route("shift")]
public sealed class ShiftController(IShiftApi api) : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return View(new ShiftIndexViewModel());
    }

    [HttpPost("redeem")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Redeem([FromForm] ShiftIndexViewModel model, CancellationToken cancellationToken = default)
    {
        var code = model.Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            ModelState.AddModelError(nameof(ShiftIndexViewModel.Code), "Enter a SHiFT code to redeem.");

            return View("Index", new ShiftIndexViewModel { Code = model.Code });
        }

        try
        {
            var response = await api.RedeemCodeAsync(code, cancellationToken);

            return View("Index", new ShiftIndexViewModel
            {
                Code = code,
                Response = response
            });
        }
        catch (Exception ex)
        {
            return View("Index", new ShiftIndexViewModel
            {
                Code = code,
                ErrorMessage = $"Redeem failed: {ex.Message}"
            });
        }
    }
}