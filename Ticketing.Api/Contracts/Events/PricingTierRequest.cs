namespace Ticketing.Api.Contracts.Events;

public sealed record PricingTierRequest(
    string Name,
    decimal Price,
    int Capacity
);
