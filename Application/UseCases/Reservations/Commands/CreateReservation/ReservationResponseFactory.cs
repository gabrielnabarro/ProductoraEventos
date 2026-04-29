using Application.DTOs;
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
            ReservedAt = reservation.ReservedAt,
            ExpiresAt = reservation.ExpiresAt,
            Message = message
        };
    }
}
