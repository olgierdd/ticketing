using System.Net;
using System.Net.Http.Json;
using Ticketing.Api.Contracts.Events;
using Ticketing.Api.Contracts.Reports;
using Ticketing.Api.Contracts.Tickets;
using Ticketing.Api.Tests.Infrastructure;

namespace Ticketing.Api.Tests;

[TestFixture]
public sealed class ReportingTests : IntegrationTestFixtureBase
{
    [Test]
    public async Task CorrectNumberOfTicketsSold_IsReported()
    {
        var eventId = await SeedSalesAsync();

        var response = await Client.GetAsync($"/api/v1/reports/events/{eventId}/sales-summary");
        var summary = await response.Content.ReadFromJsonAsync<EventSalesSummaryResponse>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(summary!.TotalTicketsSold, Is.EqualTo(5));
    }

    [Test]
    public async Task CorrectRemainingCapacity_IsReported()
    {
        var eventId = await SeedSalesAsync();

        var summary = await Client.GetFromJsonAsync<EventSalesSummaryResponse>($"/api/v1/reports/events/{eventId}/sales-summary");
        Assert.That(summary!.RemainingTickets, Is.EqualTo(summary.TotalCapacity - summary.TotalTicketsSold));
    }

    [Test]
    public async Task CorrectRevenueCalculation_IsReported()
    {
        var eventId = await SeedSalesAsync();

        var summary = await Client.GetFromJsonAsync<EventSalesSummaryResponse>($"/api/v1/reports/events/{eventId}/sales-summary");
        Assert.That(summary!.TotalRevenue, Is.EqualTo(390m));
    }

    [Test]
    public async Task GroupingByPricingTier_IsReported()
    {
        var eventId = await SeedSalesAsync();

        var summary = await Client.GetFromJsonAsync<EventSalesSummaryResponse>($"/api/v1/reports/events/{eventId}/sales-summary");

        Assert.That(summary!.SalesByTier, Has.Count.EqualTo(2));
        Assert.That(summary.SalesByTier.First(x => x.PricingTierName == "Standard").TicketsSold, Is.EqualTo(3));
        Assert.That(summary.SalesByTier.First(x => x.PricingTierName == "VIP").TicketsSold, Is.EqualTo(2));
    }

    private async Task<Guid> SeedSalesAsync()
    {
        var eventId = await CreateEventAsync(eventCapacity: 100, standardCapacity: 50, vipCapacity: 50);
        var eventResponse = await Client.GetFromJsonAsync<EventResponse>($"/api/v1/events/{eventId}");
        var standard = eventResponse!.PricingTiers.First(t => t.Name == "Standard");
        var vip = eventResponse.PricingTiers.First(t => t.Name == "VIP");

        await Client.PostAsJsonAsync("/api/v1/tickets/purchases",
            new PurchaseTicketsRequest(eventId, standard.Id, "S1", "s1@example.com", 3));
        await Client.PostAsJsonAsync("/api/v1/tickets/purchases",
            new PurchaseTicketsRequest(eventId, vip.Id, "V1", "v1@example.com", 2));

        return eventId;
    }
}

