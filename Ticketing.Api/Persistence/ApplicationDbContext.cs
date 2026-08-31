using Microsoft.EntityFrameworkCore;
using Ticketing.Api.Domain.Entities;

namespace Ticketing.Api.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<PricingTier> PricingTiers => Set<PricingTier>();
    public DbSet<TicketPurchase> TicketPurchases => Set<TicketPurchase>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2_000);
            entity.Property(e => e.Venue).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TotalTicketCapacity).IsRequired();
            entity.Property(e => e.CreatedAtUtc).IsRequired();
            entity.Property(e => e.UpdatedAtUtc).IsRequired();
            entity.HasIndex(e => e.EventDate);
            entity.Property<uint>("xmin").IsRowVersion();
        });

        modelBuilder.Entity<PricingTier>(entity =>
        {
            entity.ToTable("pricing_tiers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Price).HasPrecision(12, 2).IsRequired();
            entity.Property(e => e.Capacity).IsRequired();
            entity.Property(e => e.TicketsSold).IsRequired();
            entity.HasIndex(e => new { e.EventId, e.Name }).IsUnique();
            entity.HasOne(e => e.Event)
                .WithMany(e => e.PricingTiers)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property<uint>("xmin").IsRowVersion();
        });

        modelBuilder.Entity<TicketPurchase>(entity =>
        {
            entity.ToTable("ticket_purchases");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CustomerEmail).HasMaxLength(320).IsRequired();
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.UnitPrice).HasPrecision(12, 2).IsRequired();
            entity.Property(e => e.TotalPrice).HasPrecision(12, 2).IsRequired();
            entity.Property(e => e.PurchaseDateUtc).IsRequired();
            entity.Property(e => e.BookingReference).HasMaxLength(32).IsRequired();
            entity.HasIndex(e => e.BookingReference).IsUnique();
            entity.HasIndex(e => new { e.EventId, e.PurchaseDateUtc });
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasOne(e => e.Event)
                .WithMany(e => e.Purchases)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.PricingTier)
                .WithMany()
                .HasForeignKey(e => e.PricingTierId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property<uint>("xmin").IsRowVersion();
        });
    }
}

