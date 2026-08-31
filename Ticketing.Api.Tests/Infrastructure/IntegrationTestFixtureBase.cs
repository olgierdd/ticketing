using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Ticketing.Api.Contracts.Events;
using Ticketing.Api.Persistence;

namespace Ticketing.Api.Tests.Infrastructure;

public abstract class IntegrationTestFixtureBase
{
    private PostgreSqlContainer? _postgres;
    protected TestWebApplicationFactory Factory = null!;
    protected HttpClient Client = null!;

    [SetUp]
    public async Task SetUp()
    {
        var databaseName = $"ticketing_test_{Guid.NewGuid():N}";

        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase(databaseName)
            .WithUsername("ticketing_user")
            .WithPassword("ticketing_password")
            .Build();

        await _postgres.StartAsync();

        Factory = new TestWebApplicationFactory(_postgres.GetConnectionString());
        Client = Factory.CreateClient();

        await ResetDatabaseAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        Client.Dispose();
        await Factory.DisposeAsync();

        if (_postgres is not null)
        {
            await _postgres.StopAsync();
            await _postgres.DisposeAsync();
        }
    }

    protected async Task<Guid> CreateEventAsync(
        string name = "Summer Concert",
        int eventCapacity = 100,
        int standardCapacity = 70,
        int vipCapacity = 30)
    {
        var request = new CreateEventRequest(
            name,
            "Event description",
            "City Hall",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            new TimeOnly(20, 0),
            eventCapacity,
            [
                new PricingTierRequest("Standard", 50m, standardCapacity),
                new PricingTierRequest("VIP", 120m, vipCapacity)
            ]);

        var response = await Client.PostAsJsonAsync("/api/v1/events", request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<Ticketing.Api.Contracts.Events.EventResponse>();
        return body!.Id;
    }

    protected async Task ResetDatabaseAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
    }
}
