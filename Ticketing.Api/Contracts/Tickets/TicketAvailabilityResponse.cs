namespace Ticketing.Api.Contracts.Tickets;

public sealed record TicketAvailabilityResponse(
    Guid EventId,
    int TotalCapacity,
    int TotalTicketsSold,
    int TotalTicketsAvailable,
    IReadOnlyCollection<TierAvailabilityResponse> AvailabilityByTier
);
