using Microsoft.EntityFrameworkCore;
using Ticketing.Api.Contracts.Common;
using Ticketing.Api.Contracts.Events;
using Ticketing.Api.Domain.Entities;
using Ticketing.Api.Infrastructure.Exceptions;
using Ticketing.Api.Persistence;
using Ticketing.Api.Services.Interfaces;
using Ticketing.Api.Services.Mapping;

namespace Ticketing.Api.Services.Implementations;

public sealed class EventService(ApplicationDbContext dbContext, ILogger<EventService> logger) : IEventService
{
    public async Task<EventResponse> CreateAsync(CreateEventRequest request, CancellationToken cancellationToken)
    {
        ValidateTotalTierCapacity(request.TotalTicketCapacity, request.PricingTiers.Sum(t => t.Capacity));

        var now = DateTime.UtcNow;
        var eventEntity = new Event
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Venue = request.Venue.Trim(),
            EventDate = request.EventDate,
            StartTime = request.StartTime,
            TotalTicketCapacity = request.TotalTicketCapacity,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            PricingTiers = request.PricingTiers.Select(t => new PricingTier
            {
                Id = Guid.NewGuid(),
                Name = t.Name.Trim(),
                Price = t.Price,
                Capacity = t.Capacity,
                TicketsSold = 0
            }).ToList()
        };

        await dbContext.Events.AddAsync(eventEntity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Event created {EventId}", eventEntity.Id);

        return eventEntity.ToResponse();
    }

    public async Task<PagedResponse<EventResponse>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = dbContext.Events
            .AsNoTracking()
            .Include(e => e.PricingTiers)
            .OrderBy(e => e.EventDate)
            .ThenBy(e => e.StartTime);

        var totalItems = await query.CountAsync(cancellationToken);
        var entities = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = entities.Select(e => e.ToResponse()).ToArray();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        return new PagedResponse<EventResponse>(items, pageNumber, pageSize, totalItems, totalPages);
    }

    public async Task<EventResponse> GetByIdAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var eventEntity = await dbContext.Events
            .AsNoTracking()
            .Include(e => e.PricingTiers)
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken)
            ?? throw new NotFoundException($"Event {eventId} was not found.");

        return eventEntity.ToResponse();
    }

    public async Task<EventResponse> UpdateAsync(Guid eventId, UpdateEventRequest request, CancellationToken cancellationToken)
    {
        var eventEntity = await dbContext.Events
            .Include(e => e.PricingTiers)
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken)
            ?? throw new NotFoundException($"Event {eventId} was not found.");

        var purchasedTicketCount = eventEntity.PricingTiers.Sum(t => t.TicketsSold);
        if (request.TotalTicketCapacity < purchasedTicketCount)
        {
            throw new ConflictException("Event capacity cannot be lower than already sold tickets.");
        }

        ValidateTotalTierCapacity(request.TotalTicketCapacity, request.PricingTiers.Sum(t => t.Capacity));

        foreach (var existingTier in eventEntity.PricingTiers.ToArray())
        {
            if (!request.PricingTiers.Any(t => string.Equals(t.Name.Trim(), existingTier.Name, StringComparison.OrdinalIgnoreCase)))
            {
                if (existingTier.TicketsSold > 0)
                {
                    throw new ConflictException($"Pricing tier {existingTier.Name} cannot be removed because tickets were sold.");
                }

                dbContext.PricingTiers.Remove(existingTier);
            }
        }

        foreach (var tierRequest in request.PricingTiers)
        {
            var normalizedName = tierRequest.Name.Trim();
            var existing = eventEntity.PricingTiers.FirstOrDefault(t => string.Equals(t.Name, normalizedName, StringComparison.OrdinalIgnoreCase));
            if (existing is null)
            {
                eventEntity.PricingTiers.Add(new PricingTier
                {
                    Id = Guid.NewGuid(),
                    EventId = eventEntity.Id,
                    Name = normalizedName,
                    Price = tierRequest.Price,
                    Capacity = tierRequest.Capacity,
                    TicketsSold = 0
                });
                continue;
            }

            if (tierRequest.Capacity < existing.TicketsSold)
            {
                throw new ConflictException($"Pricing tier {existing.Name} capacity cannot be lower than sold tickets.");
            }

            existing.Name = normalizedName;
            existing.Price = tierRequest.Price;
            existing.Capacity = tierRequest.Capacity;
        }

        eventEntity.Name = request.Name.Trim();
        eventEntity.Description = request.Description.Trim();
        eventEntity.Venue = request.Venue.Trim();
        eventEntity.EventDate = request.EventDate;
        eventEntity.StartTime = request.StartTime;
        eventEntity.TotalTicketCapacity = request.TotalTicketCapacity;
        eventEntity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return eventEntity.ToResponse();
    }

    public async Task DeleteAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var eventEntity = await dbContext.Events
            .Include(e => e.PricingTiers)
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken)
            ?? throw new NotFoundException($"Event {eventId} was not found.");

        if (eventEntity.PricingTiers.Sum(t => t.TicketsSold) > 0)
        {
            throw new ConflictException("Event cannot be deleted because tickets have been sold.");
        }

        dbContext.Events.Remove(eventEntity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateTotalTierCapacity(int eventCapacity, int tiersCapacity)
    {
        if (tiersCapacity > eventCapacity)
        {
            throw new ConflictException("Combined pricing tier capacity cannot exceed total event capacity.");
        }
    }
}
