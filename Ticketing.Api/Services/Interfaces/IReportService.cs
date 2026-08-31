using Ticketing.Api.Contracts.Reports;

namespace Ticketing.Api.Services.Interfaces;

public interface IReportService
{
    Task<EventSalesSummaryResponse> GetEventSummaryAsync(Guid eventId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<EventSalesSummaryResponse>> GetAllEventSummariesAsync(CancellationToken cancellationToken);
}
