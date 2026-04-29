using Application.DTOs;
using Application.Interfaces;
using Domain.Exceptions;

namespace Application.UseCases.Reservations.Commands.CreateReservation;

public sealed class CreateReservationCommandHandler : ICreateReservationCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ReservationSelectionPolicy _reservationSelectionPolicy;
    private readonly ReservationAuditLogFactory _reservationAuditLogFactory;
    private readonly ReservationMessageFactory _reservationMessageFactory;
    private readonly ReservationResponseFactory _reservationResponseFactory;

    public CreateReservationCommandHandler(
        IUserRepository userRepository,
        ISeatRepository seatRepository,
        IReservationRepository reservationRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ReservationSelectionPolicy reservationSelectionPolicy,
        ReservationAuditLogFactory reservationAuditLogFactory,
        ReservationMessageFactory reservationMessageFactory,
        ReservationResponseFactory reservationResponseFactory)
    {
        _userRepository = userRepository;
        _seatRepository = seatRepository;
        _reservationRepository = reservationRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _reservationSelectionPolicy = reservationSelectionPolicy;
        _reservationAuditLogFactory = reservationAuditLogFactory;
        _reservationMessageFactory = reservationMessageFactory;
        _reservationResponseFactory = reservationResponseFactory;
    }

    public async Task<ReservationResponseDto> Handle(CreateReservationCommand command, CancellationToken cancellationToken = default)
    {
        ValidateCommand(command);

        var attemptTimestamp = DateTime.UtcNow;
        var userExists = await _userRepository.ExistsAsync(command.UserId, cancellationToken);

        await _auditLogRepository.AddAsync(
            _reservationAuditLogFactory.CreateAttempt(command.UserId, command.SeatId, userExists, attemptTimestamp),
            cancellationToken);

        if (!userExists)
        {
            await RejectReservationAsync(null, command.SeatId, $"Usuario {command.UserId} no encontrado.", cancellationToken);

            throw new NotFoundException("Usuario no encontrado.");
        }

        var seat = await _seatRepository.GetByIdAsync(command.SeatId, cancellationToken);

        if (seat is null)
        {
            await RejectReservationAsync(command.UserId, command.SeatId, "Butaca no encontrada.", cancellationToken);

            throw new NotFoundException("Butaca no encontrada.");
        }

        if (seat.Sector is null)
        {
            await RejectReservationAsync(command.UserId, command.SeatId, "La butaca no tiene un evento asociado.", cancellationToken);

            throw new ValidationException("La butaca no pertenece a un evento valido.");
        }

        var existingReservations = await _reservationRepository.GetPendingByUserAndEventAsync(command.UserId, seat.Sector.EventId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var selectionResult = _reservationSelectionPolicy.Apply(seat, existingReservations, command.UserId, DateTime.UtcNow);

            if (selectionResult.CreatedReservation)
            {
                await _reservationRepository.AddAsync(selectionResult.ActiveReservation, cancellationToken);
            }

            await _auditLogRepository.AddAsync(
                _reservationAuditLogFactory.CreateSuccess(command.UserId, command.SeatId, _reservationMessageFactory.CreateSuccessAuditMessage(selectionResult), DateTime.UtcNow),
                cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return _reservationResponseFactory.Create(
                selectionResult.ActiveReservation,
                seat.Status,
                _reservationMessageFactory.CreateSuccessResponseMessage(selectionResult));
        }
        catch (ConflictException)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            await RejectReservationAsync(command.UserId, command.SeatId, $"El estado de la butaca es {seat.Status}.", cancellationToken);
            throw;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task RejectReservationAsync(int? userId, Guid seatId, string details, CancellationToken cancellationToken)
    {
        await _auditLogRepository.AddAsync(
            _reservationAuditLogFactory.CreateRejected(userId, seatId, details, DateTime.UtcNow),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateCommand(CreateReservationCommand command)
    {
        if (command.SeatId == Guid.Empty)
        {
            throw new ValidationException("El identificador de la butaca es obligatorio.");
        }

        if (command.UserId <= 0)
        {
            throw new ValidationException("El identificador del usuario debe ser mayor a cero.");
        }
    }
}
