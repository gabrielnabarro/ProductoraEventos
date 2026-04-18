using Application.Interfaces;
using Application.UseCases.Reservations.Commands;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.Reservations.Handlers
{
    public class CreateReservationHandler
    {
        private readonly IReservationRepository _repository;

        public CreateReservationHandler(IReservationRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(CreateReservationCommand command)
        {
            var seat = await _repository.GetSeatByIdAsync(command.SeatId);

            if (seat == null || seat.Status != "Available") return false;

            seat.Status = "Reserved";

            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                SeatId = command.SeatId,
                UserId = command.UserId,
                Status = "Pending",
                ReservedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                Action = "RESERVE_SUCCESS",
                EntityType = "Seat",
                EntityId = command.SeatId.ToString(),
                Details = "Asiento reservado temporalmente por 5 minutos.",
                CreatedAt = DateTime.UtcNow
            };

            await _repository.SaveReservationTransactionAsync(seat, reservation, auditLog);
            return true;
        }
    }
}
