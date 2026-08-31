using Ticketing.Api.Contracts.Tickets;

namespace Ticketing.Api.Services.Interfaces;

public interface ITicketService
{
    Task<TicketAvailabilityResponse> GetAvailabilityAsync(Guid eventId, CancellationToken cancellationToken);
    Task<TicketPurchaseResponse> PurchaseAsync(PurchaseTicketsRequest request, CancellationToken cancellationToken);
    Task<TicketPurchaseResponse> GetPurchaseByIdAsync(Guid purchaseId, CancellationToken cancellationToken);
    Task<TicketPurchaseResponse> GetPurchaseByReferenceAsync(string bookingReference, CancellationToken cancellationToken);
}
