using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IReservationRepository
    {
        Task<Seat> GetSeatByIdAsync(Guid seatId);
        Task SaveReservationTransactionAsync(Seat seat, Reservation reservation, AuditLog auditLog);
    }
}
