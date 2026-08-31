namespace Ticketing.Api.Contracts.Reports;

public sealed record EventSalesByTierResponse(
    Guid PricingTierId,
    string PricingTierName,
    int TicketsSold,
    decimal Revenue
);
