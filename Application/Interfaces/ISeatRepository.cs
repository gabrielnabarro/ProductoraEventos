using Domain.Entities;

namespace Application.Interfaces;

public interface ISeatRepository
{
    Task<Seat?> GetByIdAsync(Guid seatId, CancellationToken cancellationToken = default);
}
