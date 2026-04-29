using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class SeatRepository : ISeatRepository
{
    private readonly AppDbContext _context;

    public SeatRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Seat?> GetByIdAsync(Guid seatId, CancellationToken cancellationToken = default)
    {
        return _context.SEAT
            .Include(seat => seat.Sector)
            .FirstOrDefaultAsync(seat => seat.Id == seatId, cancellationToken);
    }
}
