using Microsoft.AspNetCore.Mvc;
using Moq;
using Ticketing.Api.Contracts.Common;
using Ticketing.Api.Contracts.Events;
using Ticketing.Api.Controllers;
using Ticketing.Api.Services.Interfaces;

namespace Ticketing.Api.Tests.Unit;

[TestFixture]
public sealed class EventsControllerTests
{
    private Mock<IEventService> _eventService = null!;
    private EventsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _eventService = new Mock<IEventService>(MockBehavior.Strict);
        _controller = new EventsController(_eventService.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _eventService.VerifyAll();
    }

    [Test]
    public async Task GetEvents_ReturnsOkWithPayload()
    {
        var response = new PagedResponse<EventResponse>([], 1, 20, 0, 0);
        _eventService
            .Setup(s => s.GetAllAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.GetEvents(1, 20, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var ok = (OkObjectResult)result.Result!;
        Assert.That(ok.Value, Is.EqualTo(response));
    }
}

