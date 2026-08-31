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
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken)
            ?? throw new NotFoundException($"Event {eventId} was not found.");

        return ToSummary(eventEntity);
    }

    public async Task<IReadOnlyCollection<EventSalesSummaryResponse>> GetAllEventSummariesAsync(CancellationToken cancellationToken)
    {
        var events = await dbContext.Events
            .AsNoTracking()
            .Include(e => e.PricingTiers)
            .OrderBy(e => e.Name)
            .ToArrayAsync(cancellationToken);

        return events.Select(ToSummary).ToArray();
    }

    private static EventSalesSummaryResponse ToSummary(Domain.Entities.Event eventEntity)
    {
        var totalSold = eventEntity.PricingTiers.Sum(t => t.TicketsSold);
        var totalRevenue = eventEntity.PricingTiers.Sum(t => t.TicketsSold * t.Price);
        var byTier = eventEntity.PricingTiers
            .OrderBy(t => t.Name)
            .Select(t => new EventSalesByTierResponse(
                t.Id,
                t.Name,
                t.TicketsSold,
                t.TicketsSold * t.Price))
            .ToArray();

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
