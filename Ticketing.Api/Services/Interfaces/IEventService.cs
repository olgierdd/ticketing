using Ticketing.Api.Contracts.Common;
using Ticketing.Api.Contracts.Events;

namespace Ticketing.Api.Services.Interfaces;

public interface IEventService
{
    Task<EventResponse> CreateAsync(CreateEventRequest request, CancellationToken cancellationToken);
    Task<PagedResponse<EventResponse>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<EventResponse> GetByIdAsync(Guid eventId, CancellationToken cancellationToken);
    Task<EventResponse> UpdateAsync(Guid eventId, UpdateEventRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid eventId, CancellationToken cancellationToken);
}
