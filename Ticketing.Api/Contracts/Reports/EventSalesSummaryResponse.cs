namespace Ticketing.Api.Contracts.Reports;

public sealed record EventSalesSummaryResponse(
    Guid EventId,
    string EventName,
    int TotalCapacity,
    int TotalTicketsSold,
    int RemainingTickets,
    decimal TotalRevenue,
    IReadOnlyCollection<EventSalesByTierResponse> SalesByTier
);
