using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IEventRepository
    {
        Task<IEnumerable<Event>> GetActiveEventsAsync();
        Task<IEnumerable<Sector>> GetSectorsByEventAsync(int eventId);
        Task<IEnumerable<Seat>> GetSeatsBySectorAsync(int sectorId);
    }
}
