using Application.DTOs;
using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using System.Net;

namespace Application.UseCases.Reservations.Commands.CreateReservation;

public sealed class CreateReservationCommandHandler : ICreateReservationCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateReservationCommandHandler(
        IUserRepository userRepository,
        ISeatRepository seatRepository,
        IReservationRepository reservationRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _seatRepository = seatRepository;
        _reservationRepository = reservationRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReservationResponseDto> Handle(CreateReservationCommand command, CancellationToken cancellationToken = default)
    {
        if (command.SeatId == Guid.Empty)
        {
            throw new DomainException("SeatId is required.");
        }

        if (command.UserId <= 0)
        {
            throw new DomainException("UserId must be greater than zero.");
        }

        var attemptTimestamp = DateTime.UtcNow;
        var userExists = await _userRepository.ExistsAsync(command.UserId, cancellationToken);

        await _auditLogRepository.AddAsync(
            BuildAuditLog(
                userExists ? command.UserId : null,
                command.SeatId,
                AuditLogActions.ReserveAttempt,
                BuildAttemptDetails(command.UserId, userExists),
                attemptTimestamp),
            cancellationToken);

        if (!userExists)
        {
            await _auditLogRepository.AddAsync(
                BuildAuditLog(null, command.SeatId, AuditLogActions.ReserveRejected, $"User {command.UserId} not found.", DateTime.UtcNow),
                cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw new DomainException("User not found.", HttpStatusCode.NotFound);
        }

        var seat = await _seatRepository.GetByIdAsync(command.SeatId, cancellationToken);

        if (seat is null)
        {
            await _auditLogRepository.AddAsync(
                BuildAuditLog(command.UserId, command.SeatId, AuditLogActions.ReserveRejected, "Seat not found.", DateTime.UtcNow),
                cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw new DomainException("Seat not found.", HttpStatusCode.NotFound);
        }

        if (!seat.IsAvailable())
        {
            await _auditLogRepository.AddAsync(
                BuildAuditLog(command.UserId, command.SeatId, AuditLogActions.ReserveRejected, $"Seat status is {seat.Status}.", DateTime.UtcNow),
                cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw new DomainException("Seat is no longer available.", HttpStatusCode.Conflict);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            seat.Reserve();

            var reservationTimestamp = DateTime.UtcNow;
            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                SeatId = seat.Id,
                UserId = command.UserId,
                Status = ReservationStatuses.Reserved,
                ReservedAt = reservationTimestamp,
                ExpiresAt = reservationTimestamp.AddMinutes(5)
            };

            await _reservationRepository.AddAsync(reservation, cancellationToken);
            await _auditLogRepository.AddAsync(
                BuildAuditLog(command.UserId, command.SeatId, AuditLogActions.ReserveSuccess, "Seat reserved successfully.", DateTime.UtcNow),
                cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new ReservationResponseDto
            {
                ReservationId = reservation.Id,
                SeatId = reservation.SeatId,
                UserId = reservation.UserId,
                SeatStatus = seat.Status,
                ReservedAt = reservation.ReservedAt,
                ExpiresAt = reservation.ExpiresAt,
                Message = "Reservation created successfully."
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static string BuildAttemptDetails(int userId, bool userExists)
    {
        return userExists
            ? "Seat reservation requested."
            : $"Seat reservation requested by unknown user {userId}.";
    }

    private static AuditLog BuildAuditLog(int? userId, Guid seatId, string action, string details, DateTime timestamp)
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
}
