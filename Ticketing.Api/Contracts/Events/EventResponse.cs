namespace Ticketing.Api.Contracts.Events;

public sealed record EventResponse(
    Guid Id,
    string Name,
    string Description,
    string Venue,
    DateOnly EventDate,
    TimeOnly StartTime,
    int TotalTicketCapacity,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyCollection<PricingTierResponse> PricingTiers
);
