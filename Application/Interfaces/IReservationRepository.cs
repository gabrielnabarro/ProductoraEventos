using Domain.Entities;

namespace Application.Interfaces;

public interface IReservationRepository
{
    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Reservation>> GetByUserAsync(int userId, int? eventId = null, string? status = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Reservation>> GetExpiredPendingAsync(DateTime now, int batchSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Reservation>> GetPendingByUserAndEventAsync(int userId, int eventId, CancellationToken cancellationToken = default);
}
