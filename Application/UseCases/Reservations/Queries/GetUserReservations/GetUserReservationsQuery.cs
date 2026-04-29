namespace Application.UseCases.Reservations.Queries.GetUserReservations;

public sealed class GetUserReservationsQuery
{
    public int UserId { get; init; }
    public int? EventId { get; init; }
    public string? Status { get; init; }
}
