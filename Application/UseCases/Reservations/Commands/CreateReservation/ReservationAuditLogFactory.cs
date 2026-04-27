using Domain.Constants;
using Domain.Entities;

namespace Application.UseCases.Reservations.Commands.CreateReservation;

public sealed class ReservationAuditLogFactory
{
    public AuditLog CreateAttempt(int requestedUserId, Guid seatId, bool userExists, DateTime timestamp)
    {
        return Create(userExists ? requestedUserId : null, seatId, AuditLogActions.ReserveAttempt, BuildAttemptDetails(requestedUserId, userExists), timestamp);
    }

    public AuditLog CreateRejected(int? userId, Guid seatId, string details, DateTime timestamp)
    {
        return Create(userId, seatId, AuditLogActions.ReserveRejected, details, timestamp);
    }

    public AuditLog CreateSuccess(int userId, Guid seatId, string details, DateTime timestamp)
    {
        return Create(userId, seatId, AuditLogActions.ReserveSuccess, details, timestamp);
    }

    private static AuditLog Create(int? userId, Guid seatId, string action, string details, DateTime timestamp)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityType = AuditLogEntityTypes.Seat,
            EntityId = seatId.ToString(),
            Details = details,
            CreatedAt = timestamp
        };
    }

    private static string BuildAttemptDetails(int userId, bool userExists)
    {
        return userExists
            ? "Se solicito la reserva de la butaca."
            : $"Se solicito la reserva de la butaca por el usuario desconocido {userId}.";
    }
}
