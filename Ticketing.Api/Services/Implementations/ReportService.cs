using Microsoft.EntityFrameworkCore;
using Ticketing.Api.Contracts.Reports;
using Ticketing.Api.Infrastructure.Exceptions;
using Ticketing.Api.Persistence;
using Ticketing.Api.Services.Interfaces;

namespace Ticketing.Api.Services.Implementations;

public sealed class ReportService(ApplicationDbContext dbContext) : IReportService
{
    public async Task<EventSalesSummaryResponse> GetEventSummaryAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var eventEntity = await dbContext.Events
            .AsNoTracking()
            .Include(e => e.PricingTiers)
            .Include(e => e.Purchases)
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken)
            ?? throw new NotFoundException($"Event {eventId} was not found.");

        return ToSummary(eventEntity);
    }

    public async Task<IReadOnlyCollection<EventSalesSummaryResponse>> GetAllEventSummariesAsync(CancellationToken cancellationToken)
    {
        var events = await dbContext.Events
            .AsNoTracking()
            .Include(e => e.PricingTiers)
            .Include(e => e.Purchases)
            .OrderBy(e => e.Name)
            .ToArrayAsync(cancellationToken);

        return events.Select(ToSummary).ToArray();
    }

    private static EventSalesSummaryResponse ToSummary(Domain.Entities.Event eventEntity)
    {
        var salesByTier = eventEntity.Purchases
            .Where(p => p.Status == Domain.Entities.PurchaseStatus.Confirmed)
            .GroupBy(p => p.PricingTierId)
            .ToDictionary(
                group => group.Key,
                group => (
                    TicketsSold: group.Sum(p => p.Quantity),
                    Revenue: group.Sum(p => p.UnitPrice * p.Quantity)));

        var byTier = eventEntity.PricingTiers
            .OrderBy(t => t.Name)
            .Select(t =>
            {
                salesByTier.TryGetValue(t.Id, out var tierSales);

                return new EventSalesByTierResponse(
                    t.Id,
                    t.Name,
                    tierSales.TicketsSold,
                    tierSales.Revenue);
            })
            .ToArray();

        var totalSold = byTier.Sum(t => t.TicketsSold);
        var totalRevenue = byTier.Sum(t => t.Revenue);

        return new EventSalesSummaryResponse(
            eventEntity.Id,
            eventEntity.Name,
            eventEntity.TotalTicketCapacity,
            totalSold,
            eventEntity.TotalTicketCapacity - totalSold,
            totalRevenue,
            byTier);
    }
}
