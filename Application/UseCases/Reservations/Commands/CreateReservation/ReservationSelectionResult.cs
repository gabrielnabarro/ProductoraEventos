using Domain.Entities;

namespace Application.UseCases.Reservations.Commands.CreateReservation;

public sealed class ReservationSelectionResult
{
    public ReservationSelectionResult(Reservation activeReservation, bool createdReservation, int releasedPreviousReservationsCount)
    {
        ActiveReservation = activeReservation;
        CreatedReservation = createdReservation;
        ReleasedPreviousReservationsCount = releasedPreviousReservationsCount;
    }

    public Reservation ActiveReservation { get; }
    public bool CreatedReservation { get; }
    public int ReleasedPreviousReservationsCount { get; }
    public bool ReusedExistingReservation => !CreatedReservation;
    public bool ReleasedPreviousReservations => ReleasedPreviousReservationsCount > 0;
}
