using Ticketing.Api.Contracts.Events;
using Ticketing.Api.Contracts.Reports;
using Ticketing.Api.Contracts.Tickets;
using Ticketing.Api.Domain.Entities;

namespace Ticketing.Api.Services.Mapping;

public static class ResponseMapper
{
    public static EventResponse ToResponse(this Event entity) =>
        new(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.Venue,
            entity.EventDate,
            entity.StartTime,
            entity.TotalTicketCapacity,
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc,
            entity.PricingTiers
                .OrderBy(t => t.Name)
                .Select(t => new PricingTierResponse(t.Id, t.Name, t.Price, t.Capacity, t.TicketsSold))
                .ToArray());

    public static TicketPurchaseResponse ToResponse(this TicketPurchase entity) =>
        new(
            entity.Id,
            entity.EventId,
            entity.PricingTierId,
            entity.CustomerName,
            entity.CustomerEmail,
            entity.Quantity,
            entity.UnitPrice,
            entity.TotalPrice,
            entity.PurchaseDateUtc,
            entity.Status.ToString(),
            entity.BookingReference);

    public static EventSalesSummaryResponse ToReportResponse(
        Guid eventId,
        string eventName,
        int totalCapacity,
        int totalSold,
        decimal totalRevenue,
        IReadOnlyCollection<EventSalesByTierResponse> byTier)
    {
        var remaining = Math.Max(0, totalCapacity - totalSold);
        return new EventSalesSummaryResponse(
            eventId,
            eventName,
            totalCapacity,
            totalSold,
            remaining,
            totalRevenue,
            byTier);
    }
}

