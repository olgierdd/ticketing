namespace Ticketing.Api.Domain.Entities;

public sealed class TicketPurchase
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid PricingTierId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime PurchaseDateUtc { get; set; }
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Confirmed;
    public string BookingReference { get; set; } = string.Empty;

    public Event Event { get; set; } = null!;
    public PricingTier PricingTier { get; set; } = null!;
}
