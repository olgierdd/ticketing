namespace Ticketing.Api.Contracts.Tickets;

public sealed record PurchaseTicketsRequest(
    Guid EventId,
    Guid PricingTierId,
    string CustomerName,
    string CustomerEmail,
    int Quantity
);
