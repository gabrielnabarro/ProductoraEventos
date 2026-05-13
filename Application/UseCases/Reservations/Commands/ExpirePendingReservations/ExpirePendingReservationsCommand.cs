namespace Application.UseCases.Reservations.Commands.ExpirePendingReservations;

public sealed class ExpirePendingReservationsCommand
{
    public DateTime TimestampUtc { get; init; }
    public int BatchSize { get; init; }
}
