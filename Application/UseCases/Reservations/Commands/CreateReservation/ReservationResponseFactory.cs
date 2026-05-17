using Application.DTOs;
using Application.Common;
using Domain.Entities;

namespace Application.UseCases.Reservations.Commands.CreateReservation;

public sealed class ReservationResponseFactory
{
    public ReservationResponseDto Create(Reservation reservation, string seatStatus, string message)
    {
        return new ReservationResponseDto
        {
            ReservationId = reservation.Id,
            SeatId = reservation.SeatId,
            UserId = reservation.UserId,
            SeatStatus = seatStatus,
            ReservationStatus = reservation.Status,
            ReservedAt = UtcDateTime.Normalize(reservation.ReservedAt),
            ExpiresAt = UtcDateTime.Normalize(reservation.ExpiresAt),
            Message = message
        };
    }
}
