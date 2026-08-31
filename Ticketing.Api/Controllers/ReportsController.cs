using Microsoft.AspNetCore.Mvc;
using Ticketing.Api.Contracts.Reports;
using Ticketing.Api.Services.Interfaces;

namespace Ticketing.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Tags("Reports")]
public sealed class ReportsController(IReportService reportService) : ControllerBase
{
    /// <summary>Returns a sales summary for one event.</summary>
    [HttpGet("events/{eventId:guid}/sales-summary")]
    [ProducesResponseType<EventSalesSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventSalesSummaryResponse>> GetEventSalesSummary(Guid eventId, CancellationToken cancellationToken)
    {
        var response = await reportService.GetEventSummaryAsync(eventId, cancellationToken);
        return Ok(response);
    }

    /// <summary>Returns sales summaries for all events.</summary>
    [HttpGet("events/sales-summary")]
    [ProducesResponseType<IReadOnlyCollection<EventSalesSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<EventSalesSummaryResponse>>> GetAllSalesSummaries(CancellationToken cancellationToken)
    {
        var response = await reportService.GetAllEventSummariesAsync(cancellationToken);
        return Ok(response);
    }
}

