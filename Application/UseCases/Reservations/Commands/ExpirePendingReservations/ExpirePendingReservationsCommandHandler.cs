using Application.Interfaces;
using Domain.Exceptions;

namespace Application.UseCases.Reservations.Commands.ExpirePendingReservations;

public sealed class ExpirePendingReservationsCommandHandler : IExpirePendingReservationsCommandHandler
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ReservationExpirationAuditLogFactory _reservationExpirationAuditLogFactory;

    public ExpirePendingReservationsCommandHandler(
        IReservationRepository reservationRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ReservationExpirationAuditLogFactory reservationExpirationAuditLogFactory)
    {
        _reservationRepository = reservationRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _reservationExpirationAuditLogFactory = reservationExpirationAuditLogFactory;
    }

    public async Task<ExpirePendingReservationsResult> Handle(ExpirePendingReservationsCommand command, CancellationToken cancellationToken = default)
    {
        ValidateCommand(command);

        var reservations = await _reservationRepository.GetExpiredPendingAsync(command.TimestampUtc, command.BatchSize, cancellationToken);

        if (reservations.Count == 0)
        {
            return new ExpirePendingReservationsResult();
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var expiredReservationsCount = 0;

            foreach (var reservation in reservations)
            {
                if (!reservation.CanExpire(command.TimestampUtc))
                {
                    continue;
                }

                if (reservation.Seat is null)
                {
                    throw new InvalidOperationException($"La reserva {reservation.Id} no tiene una butaca asociada.");
                }

                reservation.Expire(command.TimestampUtc);
                reservation.Seat.Release();

                await _auditLogRepository.AddAsync(
                    _reservationExpirationAuditLogFactory.CreateExpired(reservation, command.TimestampUtc),
                    cancellationToken);

                expiredReservationsCount++;
            }

            if (expiredReservationsCount == 0)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return new ExpirePendingReservationsResult();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new ExpirePendingReservationsResult
            {
                ExpiredReservationsCount = expiredReservationsCount
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static void ValidateCommand(ExpirePendingReservationsCommand command)
    {
        if (command.BatchSize <= 0)
        {
            throw new ValidationException("El tamano del lote debe ser mayor a cero.");
        }

        if (command.TimestampUtc == default)
        {
            throw new ValidationException("El timestamp del proceso es obligatorio.");
        }
    }
}
