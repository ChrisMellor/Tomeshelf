using Microsoft.AspNetCore.Mvc;

namespace Tomeshelf.Executor.Controllers;

/// <summary>
///     MVC controller responsible for rendering the executor dashboard.
/// </summary>
public class HomeController : Controller
{
    /// <summary>
    ///     Displays the executor dashboard.
    /// </summary>
    /// <returns>The executor view.</returns>
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    ///     Handles error responses when the global exception handler is triggered.
    /// </summary>
    /// <returns>A problem details response for the current error.</returns>
    [HttpGet("/error")]
    public IActionResult Error()
    {
        var error = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()
                         ?.Error;

        return Problem(error?.Message ?? "An unexpected error occurred.");
    }
}
