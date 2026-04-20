using Domain.Entities;

namespace Application.Interfaces;

public interface IEventRepository
{
    Task<(IReadOnlyCollection<Event> Events, int TotalCount)> GetActiveEventsPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Event?> GetByIdAsync(int eventId, CancellationToken cancellationToken = default);
    Task<Event?> GetSeatMapByEventIdAsync(int eventId, CancellationToken cancellationToken = default);
}
