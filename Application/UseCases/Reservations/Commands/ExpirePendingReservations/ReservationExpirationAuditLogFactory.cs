using Domain.Constants;
using Domain.Entities;

namespace Application.UseCases.Reservations.Commands.ExpirePendingReservations;

public sealed class ReservationExpirationAuditLogFactory
{
    public AuditLog CreateExpired(Reservation reservation, DateTime timestamp)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = reservation.UserId,
            Action = AuditLogActions.ReserveExpired,
            EntityType = AuditLogEntityTypes.Seat,
            EntityId = reservation.SeatId.ToString(),
            Details = $"La reserva {reservation.Id} vencio por superar el tiempo limite de pago y la butaca fue liberada.",
            CreatedAt = timestamp
        };
    }
}
