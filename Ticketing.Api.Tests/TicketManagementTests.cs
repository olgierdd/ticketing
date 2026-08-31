using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ticketing.Api.Contracts.Events;
using Ticketing.Api.Contracts.Tickets;
using Ticketing.Api.Persistence;
using Ticketing.Api.Tests.Infrastructure;

namespace Ticketing.Api.Tests;

[TestFixture]
public sealed class TicketManagementTests : IntegrationTestFixtureBase
{
    [Test]
    public async Task PurchasingAvailableTickets_ReturnsCreated()
    {
        var eventId = await CreateEventAsync();
        var eventResponse = await Client.GetFromJsonAsync<EventResponse>($"/api/v1/events/{eventId}");
        var tier = eventResponse!.PricingTiers.First();

        var response = await Client.PostAsJsonAsync("/api/v1/tickets/purchases",
            new PurchaseTicketsRequest(eventId, tier.Id, "Alice", "alice@example.com", 2));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task CalculatingCorrectTotalPrice_IsServerSide()
    {
        var eventId = await CreateEventAsync();
        var eventResponse = await Client.GetFromJsonAsync<EventResponse>($"/api/v1/events/{eventId}");
        var tier = eventResponse!.PricingTiers.First(t => t.Name == "VIP");

        var response = await Client.PostAsJsonAsync("/api/v1/tickets/purchases",
            new PurchaseTicketsRequest(eventId, tier.Id, "Alice", "alice@example.com", 3));

        var purchase = await response.Content.ReadFromJsonAsync<TicketPurchaseResponse>();
        Assert.That(purchase!.UnitPrice, Is.EqualTo(tier.Price));
        Assert.That(purchase.TotalPrice, Is.EqualTo(tier.Price * 3));
    }

    [Test]
    public async Task RejectingInvalidPricingTier_ReturnsBadRequest()
    {
        var eventId = await CreateEventAsync();
        var response = await Client.PostAsJsonAsync("/api/v1/tickets/purchases",
            new PurchaseTicketsRequest(eventId, Guid.NewGuid(), "Bob", "bob@example.com", 1));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task RejectingPurchasesExceedingTierCapacity_ReturnsConflict()
    {
        var eventId = await CreateEventAsync(standardCapacity: 2, vipCapacity: 2, eventCapacity: 5);
        var eventResponse = await Client.GetFromJsonAsync<EventResponse>($"/api/v1/events/{eventId}");
        var tier = eventResponse!.PricingTiers.First(t => t.Name == "Standard");

        var response = await Client.PostAsJsonAsync("/api/v1/tickets/purchases",
            new PurchaseTicketsRequest(eventId, tier.Id, "Eve", "eve@example.com", 3));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task RejectingPurchasesExceedingEventCapacity_ReturnsConflict()
    {
        var eventId = await CreateEventAsync(eventCapacity: 5, standardCapacity: 3, vipCapacity: 2);
        var eventResponse = await Client.GetFromJsonAsync<EventResponse>($"/api/v1/events/{eventId}");
        var standard = eventResponse!.PricingTiers.First(t => t.Name == "Standard");
        var vip = eventResponse.PricingTiers.First(t => t.Name == "VIP");

        var first = await Client.PostAsJsonAsync("/api/v1/tickets/purchases",
            new PurchaseTicketsRequest(eventId, standard.Id, "Eve", "eve@example.com", 3));
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var eventEntity = await dbContext.Events.FirstAsync(e => e.Id == eventId);
            eventEntity.TotalTicketCapacity = 4;
            await dbContext.SaveChangesAsync();
        }

        var second = await Client.PostAsJsonAsync("/api/v1/tickets/purchases",
            new PurchaseTicketsRequest(eventId, vip.Id, "Eve", "eve@example.com", 2));

        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task PreventingOversellingDuringConcurrentRequests_ReturnsOneConflict()
    {
        var eventId = await CreateEventAsync(eventCapacity: 3, standardCapacity: 2, vipCapacity: 1);
        var eventResponse = await Client.GetFromJsonAsync<EventResponse>($"/api/v1/events/{eventId}");
        var tier = eventResponse!.PricingTiers.First(t => t.Name == "Standard");

        var request = new PurchaseTicketsRequest(eventId, tier.Id, "Concurrent", "c@example.com", 2);

        var t1 = Client.PostAsJsonAsync("/api/v1/tickets/purchases", request);
        var t2 = Client.PostAsJsonAsync("/api/v1/tickets/purchases", request);

        await Task.WhenAll(t1, t2);

        var statuses = new[] { t1.Result.StatusCode, t2.Result.StatusCode };
        Assert.That(statuses.Count(s => s == HttpStatusCode.Created), Is.EqualTo(1));
        Assert.That(statuses.Count(s => s == HttpStatusCode.Conflict), Is.EqualTo(1));
    }

    [Test]
    public async Task ReturningCorrectTicketAvailability_Works()
    {
        var eventId = await CreateEventAsync();
        var eventResponse = await Client.GetFromJsonAsync<EventResponse>($"/api/v1/events/{eventId}");
        var tier = eventResponse!.PricingTiers.First(t => t.Name == "Standard");

        await Client.PostAsJsonAsync("/api/v1/tickets/purchases",
            new PurchaseTicketsRequest(eventId, tier.Id, "A", "a@a.com", 4));

        var availabilityResponse = await Client.GetAsync($"/api/v1/tickets/availability/{eventId}");
        var availability = await availabilityResponse.Content.ReadFromJsonAsync<TicketAvailabilityResponse>();

        Assert.That(availabilityResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(availability!.TotalTicketsSold, Is.EqualTo(4));
    }

    [Test]
    public async Task GeneratingUniqueBookingReference_ReturnsDifferentValues()
    {
        var eventId = await CreateEventAsync(eventCapacity: 11, standardCapacity: 10, vipCapacity: 1);
        var eventResponse = await Client.GetFromJsonAsync<EventResponse>($"/api/v1/events/{eventId}");
        var tier = eventResponse!.PricingTiers.First(t => t.Name == "Standard");

        var firstResponse = await Client.PostAsJsonAsync("/api/v1/tickets/purchases",
            new PurchaseTicketsRequest(eventId, tier.Id, "First", "first@example.com", 1));
        var secondResponse = await Client.PostAsJsonAsync("/api/v1/tickets/purchases",
            new PurchaseTicketsRequest(eventId, tier.Id, "Second", "second@example.com", 1));

        var first = await firstResponse.Content.ReadFromJsonAsync<TicketPurchaseResponse>();
        var second = await secondResponse.Content.ReadFromJsonAsync<TicketPurchaseResponse>();

        Assert.That(first!.BookingReference, Is.Not.EqualTo(second!.BookingReference));
    }
}
