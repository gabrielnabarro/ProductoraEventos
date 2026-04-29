namespace Application.UseCases.Reservations.Commands.CreateReservation;

public sealed class CreateReservationCommand
{
    public Guid SeatId { get; init; }
    public int UserId { get; init; }
}
