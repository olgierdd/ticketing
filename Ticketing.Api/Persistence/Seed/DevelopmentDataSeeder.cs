using Microsoft.EntityFrameworkCore;
using Ticketing.Api.Domain.Entities;

namespace Ticketing.Api.Persistence.Seed;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.Events.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var events = new[]
        {
            CreateEvent("Nordic Lights Live", "Main Hall", now, 14),
            CreateEvent("Symphonic Evenings", "City Arena", now, 30),
            CreateEvent("Jazz Under Stars", "River Stage", now, 45)
        };

        await dbContext.Events.AddRangeAsync(events, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Event CreateEvent(string name, string venue, DateTime now, int daysFromNow)
    {
        var eventId = Guid.NewGuid();
        return new Event
        {
            Id = eventId,
            Name = name,
            Description = $"{name} featuring three audience tiers.",
            Venue = venue,
            EventDate = DateOnly.FromDateTime(now.Date.AddDays(daysFromNow)),
            StartTime = new TimeOnly(19, 30),
            TotalTicketCapacity = 300,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            PricingTiers =
            [
                new PricingTier { Id = Guid.NewGuid(), EventId = eventId, Name = "Standard", Price = 49.99m, Capacity = 180, TicketsSold = 0 },
                new PricingTier { Id = Guid.NewGuid(), EventId = eventId, Name = "Premium", Price = 89.99m, Capacity = 90, TicketsSold = 0 },
                new PricingTier { Id = Guid.NewGuid(), EventId = eventId, Name = "VIP", Price = 149.99m, Capacity = 30, TicketsSold = 0 }
            ]
        };
    }
}

