using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;

    public EventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyCollection<Event> Events, int TotalCount)> GetActiveEventsPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.EVENT
            .AsNoTracking()
            .Where(eventEntity => eventEntity.Status == EventStatuses.Active)
            .OrderBy(eventEntity => eventEntity.EventDate)
            .ThenBy(eventEntity => eventEntity.Id);

        var totalCount = await query.CountAsync(cancellationToken);
        var events = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (events, totalCount);
    }

    public Task<Event?> GetByIdAsync(int eventId, CancellationToken cancellationToken = default)
    {
        return _context.EVENT
            .AsNoTracking()
            .FirstOrDefaultAsync(eventEntity => eventEntity.Id == eventId, cancellationToken);
    }

    public Task<Event?> GetSeatMapByEventIdAsync(int eventId, CancellationToken cancellationToken = default)
    {
        return _context.EVENT
            .AsNoTracking()
            .Include(eventEntity => eventEntity.Sectors.OrderBy(sector => sector.Id))
            .ThenInclude(sector => sector.Seats.OrderBy(seat => seat.RowIdentifier).ThenBy(seat => seat.SeatNumber))
            .AsSplitQuery()
            .FirstOrDefaultAsync(eventEntity => eventEntity.Id == eventId, cancellationToken);
    }
}
