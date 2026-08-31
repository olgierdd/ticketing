namespace Ticketing.Api.Domain.Entities;

public sealed class PricingTier
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Capacity { get; set; }
    public int TicketsSold { get; set; }

    public Event Event { get; set; } = null!;
}
