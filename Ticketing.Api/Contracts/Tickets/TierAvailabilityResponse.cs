namespace Ticketing.Api.Contracts.Tickets;

public sealed record TierAvailabilityResponse(
    Guid PricingTierId,
    string Name,
    int Capacity,
    int TicketsSold,
    int TicketsAvailable
);
