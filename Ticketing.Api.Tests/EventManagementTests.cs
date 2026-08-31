using System.Net;
using System.Net.Http.Json;
using Ticketing.Api.Contracts.Events;
using Ticketing.Api.Contracts.Tickets;
using Ticketing.Api.Tests.Infrastructure;

namespace Ticketing.Api.Tests;

[TestFixture]
public sealed class EventManagementTests : IntegrationTestFixtureBase
{
    [Test]
    public async Task CreatingAValidEvent_ReturnsCreated()
    {
        var request = new CreateEventRequest(
            "Rock Night",
            "Live concert",
            "Grand Arena",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)),
            new TimeOnly(19, 0),
            200,
            [
                new PricingTierRequest("Standard", 55m, 150),
                new PricingTierRequest("VIP", 120m, 50)
            ]);

        var response = await Client.PostAsJsonAsync("/api/v1/events", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var payload = await response.Content.ReadFromJsonAsync<EventResponse>();
        Assert.That(payload, Is.Not.Null);
        Assert.That(payload!.PricingTiers, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task RejectingInvalidEventInput_ReturnsBadRequest()
    {
        var request = new CreateEventRequest(
            string.Empty,
            "desc",
            string.Empty,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            new TimeOnly(18, 0),
            0,
            []);

        var response = await Client.PostAsJsonAsync("/api/v1/events", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task RetrievingAnExistingEvent_ReturnsEvent()
    {
        var eventId = await CreateEventAsync();

        var response = await Client.GetAsync($"/api/v1/events/{eventId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var payload = await response.Content.ReadFromJsonAsync<EventResponse>();
        Assert.That(payload!.Id, Is.EqualTo(eventId));
    }

    [Test]
    public async Task Returning404ForMissingEvent_Works()
    {
        var response = await Client.GetAsync($"/api/v1/events/{Guid.NewGuid()}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task UpdatingAnEvent_ReturnsUpdatedEntity()
    {
        var eventId = await CreateEventAsync();
        var update = new UpdateEventRequest(
            "Updated Concert",
            "Updated description",
            "Updated Venue",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            new TimeOnly(21, 0),
            150,
            [
                new PricingTierRequest("Standard", 49m, 100),
                new PricingTierRequest("VIP", 140m, 50)
            ]);

        var response = await Client.PutAsJsonAsync($"/api/v1/events/{eventId}", update);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var payload = await response.Content.ReadFromJsonAsync<EventResponse>();
        Assert.That(payload!.Name, Is.EqualTo("Updated Concert"));
    }

    [Test]
    public async Task DeletingAnEvent_ReturnsNoContent()
    {
        var eventId = await CreateEventAsync(name: "Delete Me");

        var deleteResponse = await Client.DeleteAsync($"/api/v1/events/{eventId}");
        var getResponse = await Client.GetAsync($"/api/v1/events/{eventId}");

        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task PreventingInvalidCapacityChanges_ReturnsConflict()
    {
        var eventId = await CreateEventAsync();
        var getEvent = await Client.GetFromJsonAsync<EventResponse>($"/api/v1/events/{eventId}");
        var standardTier = getEvent!.PricingTiers.First(t => t.Name == "Standard");

        var purchase = new PurchaseTicketsRequest(eventId, standardTier.Id, "Jane Doe", "jane@example.com", 5);
        var purchaseResponse = await Client.PostAsJsonAsync("/api/v1/tickets/purchases", purchase);
        Assert.That(purchaseResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var update = new UpdateEventRequest(
            getEvent.Name,
            getEvent.Description,
            getEvent.Venue,
            getEvent.EventDate,
            getEvent.StartTime,
            4,
            [
                new PricingTierRequest("Standard", 50m, 3),
                new PricingTierRequest("VIP", 120m, 1)
            ]);

        var response = await Client.PutAsJsonAsync($"/api/v1/events/{eventId}", update);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }
}
