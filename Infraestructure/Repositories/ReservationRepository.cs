using Application.Interfaces;
using Domain.Entities;
using Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructure.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly ApiDBContext _context;
        public ReservationRepository(ApiDBContext context) => _context = context;

        public async Task<Seat?> GetSeatByIdAsync(Guid seatId)
        {
            return await _context.Seat.FirstOrDefaultAsync(s => s.Id == seatId);
        }

        public async Task SaveReservationTransactionAsync(Seat seat, Reservation reservation, AuditLog auditLog)
        {
            _context.Seat.Update(seat);
            _context.Reservation.Add(reservation);
            _context.AuditLog.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}
