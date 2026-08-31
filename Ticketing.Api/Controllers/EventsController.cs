using Microsoft.AspNetCore.Mvc;
using Ticketing.Api.Contracts.Common;
using Ticketing.Api.Contracts.Events;
using Ticketing.Api.Services.Interfaces;

namespace Ticketing.Api.Controllers;

[ApiController]
[Route("api/v1/events")]
[Tags("Events")]
public sealed class EventsController(IEventService eventService) : ControllerBase
{
    /// <summary>Creates a new event with pricing tiers.</summary>
    [HttpPost]
    [ProducesResponseType<EventResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EventResponse>> CreateEvent([FromBody] CreateEventRequest request, CancellationToken cancellationToken)
    {
        var created = await eventService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetEventById), new { eventId = created.Id }, created);
    }

    /// <summary>Gets all events using pagination.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResponse<EventResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<EventResponse>>> GetEvents([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var events = await eventService.GetAllAsync(pageNumber, pageSize, cancellationToken);
        return Ok(events);
    }

    /// <summary>Gets a single event by id.</summary>
    [HttpGet("{eventId:guid}")]
    [ProducesResponseType<EventResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> GetEventById(Guid eventId, CancellationToken cancellationToken)
    {
        var eventResponse = await eventService.GetByIdAsync(eventId, cancellationToken);
        return Ok(eventResponse);
    }

    /// <summary>Replaces an existing event and pricing tiers.</summary>
    [HttpPut("{eventId:guid}")]
    [ProducesResponseType<EventResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EventResponse>> UpdateEvent(Guid eventId, [FromBody] UpdateEventRequest request, CancellationToken cancellationToken)
    {
        var updated = await eventService.UpdateAsync(eventId, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>Deletes an event when no tickets have been sold.</summary>
    [HttpDelete("{eventId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteEvent(Guid eventId, CancellationToken cancellationToken)
    {
        await eventService.DeleteAsync(eventId, cancellationToken);
        return NoContent();
    }
}

