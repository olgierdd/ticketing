namespace Ticketing.Api.Contracts.Events;

public sealed record PricingTierResponse(
    Guid Id,
    string Name,
    decimal Price,
    int Capacity,
    int TicketsSold
);
