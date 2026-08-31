namespace Ticketing.Api.Contracts.Events;

public sealed record CreateEventRequest(
    string Name,
    string Description,
    string Venue,
    DateOnly EventDate,
    TimeOnly StartTime,
    int TotalTicketCapacity,
    IReadOnlyCollection<PricingTierRequest> PricingTiers
);
