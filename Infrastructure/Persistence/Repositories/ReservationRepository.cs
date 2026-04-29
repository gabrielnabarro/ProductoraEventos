using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _context;

    public ReservationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        await _context.RESERVATION.AddAsync(reservation, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Reservation>> GetByUserAsync(int userId, int? eventId = null, string? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.RESERVATION
            .Include(reservation => reservation.Seat)
                .ThenInclude(seat => seat!.Sector)
            .Where(reservation => reservation.UserId == userId && reservation.Seat != null && reservation.Seat.Sector != null);

        if (eventId.HasValue)
        {
            query = query.Where(reservation => reservation.Seat!.Sector!.EventId == eventId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(reservation => reservation.Status == status);
        }

        return await query
            .OrderByDescending(reservation => reservation.ReservedAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Reservation>> GetPendingByUserAndEventAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        return await GetByUserAsync(userId, eventId, ReservationStatuses.Pending, cancellationToken);
    }
}
