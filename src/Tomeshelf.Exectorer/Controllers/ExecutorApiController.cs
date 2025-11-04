using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Executor.Models;
using Tomeshelf.Executor.Services;

namespace Tomeshelf.Executor.Controllers;

/// <summary>
///     API surface for interacting with executor endpoints.
/// </summary>
[ApiController]
[Route("api/executor")]
public class ExecutorApiController : ControllerBase
{
    private readonly EndpointCatalog _catalog;
    private readonly EndpointExecutor _executor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExecutorApiController" /> class.
    /// </summary>
    /// <param name="catalog">Endpoint catalog for discovery.</param>
    /// <param name="executor">Executor used to run endpoints.</param>
    public ExecutorApiController(EndpointCatalog catalog, EndpointExecutor executor)
    {
        _catalog = catalog;
        _executor = executor;
    }

    /// <summary>
    ///     Retrieves the list of configured executor endpoints.
    /// </summary>
    /// <returns>A list of endpoint summaries.</returns>
    [HttpGet("endpoints")]
    public ActionResult<IEnumerable<EndpointSummary>> GetEndpoints()
    {
        var summaries = _catalog.GetSummaries();

        return Ok(summaries);
    }

    /// <summary>
    ///     Returns the upcoming scheduled runs for a specific endpoint.
    /// </summary>
    /// <param name="id">Endpoint identifier.</param>
    /// <returns>Recent upcoming occurrences.</returns>
    [HttpGet("endpoints/{id}/next")]
    public ActionResult GetUpcomingRuns(string id)
    {
        if (!_catalog.TryGetDescriptor(id, out var descriptor))
        {
            return NotFound();
        }

        if (descriptor.CronExpression is null)
        {
            return NoContent();
        }

        var occurrences = new List<DateTimeOffset>();
        var reference = DateTimeOffset.UtcNow;
        var timeZoneId = descriptor.TimeZone?.Id ?? descriptor.CronExpression.TimeZone?.Id ?? TimeZoneInfo.Utc.Id;

        for (var i = 0; i < 5; i++)
        {
            var next = descriptor.CronExpression.GetNextValidTimeAfter(reference);
            if (next is null)
            {
                break;
            }

            occurrences.Add(next.Value);
            reference = next.Value;
        }

        return Ok(new
        {
            timeZone = timeZoneId,
            occurrences
        });
    }

    /// <summary>
    ///     Executes an endpoint immediately with optional overrides.
    /// </summary>
    /// <param name="id">Endpoint identifier.</param>
    /// <param name="request">Optional execution overrides.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The execution result.</returns>
    [HttpPost("endpoints/{id}/execute")]
    public async Task<IActionResult> ExecuteEndpoint(string id, EndpointExecutionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _executor.ExecuteAsync(id, request, cancellationToken)
                                        .ConfigureAwait(false);

            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
