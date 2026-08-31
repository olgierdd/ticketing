using Microsoft.AspNetCore.Mvc;
using Ticketing.Api.Contracts.Tickets;
using Ticketing.Api.Services.Interfaces;

namespace Ticketing.Api.Controllers;

[ApiController]
[Route("api/v1/tickets")]
[Tags("Tickets")]
public sealed class TicketsController(ITicketService ticketService) : ControllerBase
{
    /// <summary>Returns capacity and availability for the event.</summary>
    [HttpGet("availability/{eventId:guid}")]
    [ProducesResponseType<TicketAvailabilityResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketAvailabilityResponse>> GetAvailability(Guid eventId, CancellationToken cancellationToken)
    {
        var response = await ticketService.GetAvailabilityAsync(eventId, cancellationToken);
        return Ok(response);
    }

    /// <summary>Purchases tickets for one event and pricing tier.</summary>
    [HttpPost("purchases")]
    [ProducesResponseType<TicketPurchaseResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketPurchaseResponse>> Purchase([FromBody] PurchaseTicketsRequest request, CancellationToken cancellationToken)
    {
        var response = await ticketService.PurchaseAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPurchaseById), new { purchaseId = response.Id }, response);
    }

    /// <summary>Gets purchase details by purchase id.</summary>
    [HttpGet("purchases/{purchaseId:guid}")]
    [ProducesResponseType<TicketPurchaseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketPurchaseResponse>> GetPurchaseById(Guid purchaseId, CancellationToken cancellationToken)
    {
        var response = await ticketService.GetPurchaseByIdAsync(purchaseId, cancellationToken);
        return Ok(response);
    }

    /// <summary>Gets purchase details by booking reference.</summary>
    [HttpGet("purchases/reference/{bookingReference}")]
    [ProducesResponseType<TicketPurchaseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketPurchaseResponse>> GetPurchaseByReference(string bookingReference, CancellationToken cancellationToken)
    {
        var response = await ticketService.GetPurchaseByReferenceAsync(bookingReference, cancellationToken);
        return Ok(response);
    }
}

