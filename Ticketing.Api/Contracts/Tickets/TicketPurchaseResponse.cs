namespace Ticketing.Api.Contracts.Tickets;

public sealed record TicketPurchaseResponse(
    Guid Id,
    Guid EventId,
    Guid PricingTierId,
    string CustomerName,
    string CustomerEmail,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    DateTime PurchaseDateUtc,
    string Status,
    string BookingReference
);
