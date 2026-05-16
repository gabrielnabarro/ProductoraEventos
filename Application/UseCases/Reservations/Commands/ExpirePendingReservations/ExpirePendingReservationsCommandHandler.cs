using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.UseCases.Reservations.Commands.ExpirePendingReservations;

public sealed class ExpirePendingReservationsCommandHandler : IExpirePendingReservationsCommandHandler
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ExpirePendingReservationsCommandHandler(
        IReservationRepository reservationRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(ExpirePendingReservationsCommand command, CancellationToken cancellationToken = default)
    {
        ValidateCommand(command);

        var timestampUtc = DateTime.UtcNow;
        var reservations = await _reservationRepository.GetExpiredPendingAsync(timestampUtc, command.BatchSize, cancellationToken);

        if (reservations.Count == 0)
        {
            return 0;
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var expiredReservationsCount = 0;

            foreach (var reservation in reservations)
            {
                if (!reservation.CanExpire(timestampUtc))
                {
                    continue;
                }

                if (reservation.Seat is null)
                {
                    throw new InvalidOperationException($"La reserva {reservation.Id} no tiene una butaca asociada.");
                }

                reservation.Expire(timestampUtc);
                reservation.Seat.Release();

                await _auditLogRepository.AddAsync(
                    CreateExpiredAuditLog(reservation, timestampUtc),
                    cancellationToken);

                expiredReservationsCount++;
            }

            if (expiredReservationsCount == 0)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return 0;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return expiredReservationsCount;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static AuditLog CreateExpiredAuditLog(Reservation reservation, DateTime timestampUtc)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = reservation.UserId,
            Action = AuditLogActions.ReserveExpired,
            EntityType = AuditLogEntityTypes.Seat,
            EntityId = reservation.SeatId.ToString(),
            Details = $"La reserva {reservation.Id} vencio por superar el tiempo limite de pago y la butaca fue liberada.",
            CreatedAt = timestampUtc
        };
    }

    private static void ValidateCommand(ExpirePendingReservationsCommand command)
    {
        if (command.BatchSize <= 0)
        {
            throw new ValidationException("El tamaño del lote debe ser mayor a cero.");
        }
    }
}
