namespace Ticketing.Api.Domain.Entities;

public sealed class Event
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public DateOnly EventDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public int TotalTicketCapacity { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public List<PricingTier> PricingTiers { get; set; } = [];
    public List<TicketPurchase> Purchases { get; set; } = [];
}
