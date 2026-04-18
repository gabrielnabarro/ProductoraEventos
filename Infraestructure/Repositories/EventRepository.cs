using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces;
using Infraestructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly ApiDBContext _context;

        public EventRepository(ApiDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Event>> GetActiveEventsAsync()
        {
            return await _context.Event
                .Where(e => e.Status == "Active")
                .ToListAsync();
        }

        public async Task<IEnumerable<Sector>> GetSectorsByEventAsync(int eventId)
        {
            return await _context.Sector
                .Where(s => s.EventId == eventId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Seat>> GetSeatsBySectorAsync(int sectorId)
        {
            return await _context.Seat
                .Where(s => s.SectorId == sectorId)
                .ToListAsync();
        }
    }
}
