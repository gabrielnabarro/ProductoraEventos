using Domain.Entities;
using Domain.Exceptions;

namespace Application.UseCases.Reservations.Commands.CreateReservation;

public sealed class ReservationSelectionPolicy
{
    public ReservationSelectionResult Apply(Seat seat, IReadOnlyCollection<Reservation> existingReservations, int userId, DateTime timestamp)
    {
        var currentReservation = existingReservations.FirstOrDefault(reservation => reservation.IsForSeat(seat.Id));
        var previousReservations = existingReservations.Where(reservation => !reservation.IsForSeat(seat.Id)).ToArray();

        if (currentReservation is null && !seat.IsAvailable())
        {
            throw new ConflictException("La butaca ya no esta disponible.");
        }

        foreach (var previousReservation in previousReservations)
        {
            previousReservation.Expire(timestamp);
            previousReservation.Seat?.Release();
        }

        if (currentReservation is not null)
        {
            if (seat.IsAvailable())
            {
                seat.Reserve();
            }

            return new ReservationSelectionResult(currentReservation, createdReservation: false, previousReservations.Length);
        }

        seat.Reserve();

        var reservation = Reservation.CreatePending(userId, seat.Id, timestamp, timestamp.AddMinutes(5));

        return new ReservationSelectionResult(reservation, createdReservation: true, previousReservations.Length);
    }
}
