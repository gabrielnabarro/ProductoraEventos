namespace Application.UseCases.Reservations.Commands.ExpirePendingReservations;

public sealed class ExpirePendingReservationsCommand
{
    public int BatchSize { get; init; }
}
