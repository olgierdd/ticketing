using System.Data;
using Microsoft.EntityFrameworkCore;
using Ticketing.Api.Contracts.Tickets;
using Ticketing.Api.Domain.Entities;
using Ticketing.Api.Infrastructure.Exceptions;
using Ticketing.Api.Persistence;
using Ticketing.Api.Services.Interfaces;
using Ticketing.Api.Services.Mapping;

namespace Ticketing.Api.Services.Implementations;

public sealed class TicketService(ApplicationDbContext dbContext, ILogger<TicketService> logger) : ITicketService
{
    public async Task<TicketAvailabilityResponse> GetAvailabilityAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var eventEntity = await dbContext.Events
            .AsNoTracking()
            .Include(e => e.PricingTiers)
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken)
            ?? throw new NotFoundException($"Event {eventId} was not found.");

        var sold = eventEntity.PricingTiers.Sum(t => t.TicketsSold);
        var byTier = eventEntity.PricingTiers
            .OrderBy(t => t.Name)
            .Select(t => new TierAvailabilityResponse(
                t.Id,
                t.Name,
                t.Capacity,
                t.TicketsSold,
                t.Capacity - t.TicketsSold))
            .ToArray();

        return new TicketAvailabilityResponse(
            eventEntity.Id,
            eventEntity.TotalTicketCapacity,
            sold,
            eventEntity.TotalTicketCapacity - sold,
            byTier);
    }

    public async Task<TicketPurchaseResponse> PurchaseAsync(PurchaseTicketsRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);

        // Explicit row lock prevents concurrent inventory updates for the same event.
        var lockCount = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM events WHERE \"Id\" = {request.EventId} FOR UPDATE",
            cancellationToken);

        if (lockCount == 0)
        {
            throw new NotFoundException($"Event {request.EventId} was not found.");
        }

        var lockedEvent = await dbContext.Events
            .Include(e => e.PricingTiers)
            .FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken)
            ?? throw new NotFoundException($"Event {request.EventId} was not found.");

        var tier = lockedEvent.PricingTiers.FirstOrDefault(t => t.Id == request.PricingTierId)
            ?? throw new BusinessRuleViolationException("Pricing tier does not belong to the selected event.");

        var eventDateTimeUtc = lockedEvent.EventDate.ToDateTime(lockedEvent.StartTime, DateTimeKind.Utc);
        if (eventDateTimeUtc <= DateTime.UtcNow)
        {
            throw new BusinessRuleViolationException("Tickets cannot be purchased for an event that has already started.");
        }

        var totalSold = lockedEvent.PricingTiers.Sum(t => t.TicketsSold);
        if (tier.TicketsSold + request.Quantity > tier.Capacity)
        {
            throw new ConflictException("Not enough capacity in selected pricing tier.");
        }

        if (totalSold + request.Quantity > lockedEvent.TotalTicketCapacity)
        {
            throw new ConflictException("Not enough event capacity.");
        }

        tier.TicketsSold += request.Quantity;
        lockedEvent.UpdatedAtUtc = DateTime.UtcNow;

        var purchase = new TicketPurchase
        {
            Id = Guid.NewGuid(),
            EventId = lockedEvent.Id,
            PricingTierId = tier.Id,
            CustomerName = request.CustomerName.Trim(),
            CustomerEmail = request.CustomerEmail.Trim(),
            Quantity = request.Quantity,
            UnitPrice = tier.Price,
            TotalPrice = tier.Price * request.Quantity,
            PurchaseDateUtc = DateTime.UtcNow,
            Status = PurchaseStatus.Confirmed,
            BookingReference = await GenerateBookingReferenceAsync(cancellationToken)
        };

        await dbContext.TicketPurchases.AddAsync(purchase, cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ConflictException("Inventory changed while processing the purchase.");
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogWarning(ex, "Db update failed while creating a purchase for event {EventId}", request.EventId);
            throw;
        }

        return purchase.ToResponse();
    }

    public async Task<TicketPurchaseResponse> GetPurchaseByIdAsync(Guid purchaseId, CancellationToken cancellationToken)
    {
        var purchase = await dbContext.TicketPurchases
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == purchaseId, cancellationToken)
            ?? throw new NotFoundException($"Purchase {purchaseId} was not found.");

        return purchase.ToResponse();
    }

    public async Task<TicketPurchaseResponse> GetPurchaseByReferenceAsync(string bookingReference, CancellationToken cancellationToken)
    {
        var purchase = await dbContext.TicketPurchases
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.BookingReference == bookingReference, cancellationToken)
            ?? throw new NotFoundException($"Purchase with booking reference {bookingReference} was not found.");

        return purchase.ToResponse();
    }

    private async Task<string> GenerateBookingReferenceAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var value = $"BK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..22].ToUpperInvariant();
            var exists = await dbContext.TicketPurchases.AnyAsync(p => p.BookingReference == value, cancellationToken);
            if (!exists)
            {
                return value;
            }
        }

        throw new ConflictException("Could not generate unique booking reference.");
    }
}
