namespace Ticketing.Api.Contracts.Events;

public sealed record UpdateEventRequest(
    string Name,
    string Description,
    string Venue,
    DateOnly EventDate,
    TimeOnly StartTime,
    int TotalTicketCapacity,
    IReadOnlyCollection<PricingTierRequest> PricingTiers
);
