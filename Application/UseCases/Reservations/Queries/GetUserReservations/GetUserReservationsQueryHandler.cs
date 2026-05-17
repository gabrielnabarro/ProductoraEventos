using Application.DTOs;
using Application.Common;
using Application.Interfaces;
using Domain.Constants;
using Domain.Exceptions;

namespace Application.UseCases.Reservations.Queries.GetUserReservations;

public sealed class GetUserReservationsQueryHandler : IGetUserReservationsQueryHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IReservationRepository _reservationRepository;

    public GetUserReservationsQueryHandler(IUserRepository userRepository, IReservationRepository reservationRepository)
    {
        _userRepository = userRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<IReadOnlyCollection<UserReservationResponseDto>> Handle(GetUserReservationsQuery query, CancellationToken cancellationToken = default)
    {
        if (query.UserId <= 0)
        {
            throw new ValidationException("El identificador del usuario debe ser mayor a cero.");
        }

        if (query.EventId.HasValue && query.EventId.Value <= 0)
        {
            throw new ValidationException("El identificador del evento debe ser mayor a cero.");
        }

        if (!await _userRepository.ExistsAsync(query.UserId, cancellationToken))
        {
            throw new NotFoundException("Usuario no encontrado.");
        }

        var status = NormalizeStatus(query.Status);
        var reservations = await _reservationRepository.GetByUserAsync(query.UserId, query.EventId, status, cancellationToken);

        return reservations
            .Select(MapReservation)
            .ToArray();
    }

    private static string? NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return status.Trim().ToLowerInvariant() switch
        {
            "pending" => ReservationStatuses.Pending,
            "paid" => ReservationStatuses.Paid,
            "expired" => ReservationStatuses.Expired,
            _ => throw new ValidationException("El estado de la reserva no es valido.")
        };
    }

    private static UserReservationResponseDto MapReservation(Domain.Entities.Reservation reservation)
    {
        var seat = reservation.Seat!;
        var sector = seat.Sector!;

        return new UserReservationResponseDto
        {
            ReservationId = reservation.Id,
            UserId = reservation.UserId,
            EventId = sector.EventId,
            SeatId = reservation.SeatId,
            SectorId = sector.Id,
            SectorName = sector.Name,
            Price = sector.Price,
            SeatStatus = seat.Status,
            SeatRowIdentifier = seat.RowIdentifier,
            SeatNumber = seat.SeatNumber,
            ReservationStatus = reservation.Status,
            ReservedAt = UtcDateTime.Normalize(reservation.ReservedAt),
            ExpiresAt = UtcDateTime.Normalize(reservation.ExpiresAt)
        };
    }
}
