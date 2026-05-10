namespace Application.UseCases.Reservations.Commands.ExpirePendingReservations;

public sealed class ExpirePendingReservationsResult
{
    public int ExpiredReservationsCount { get; init; }
}
