namespace Application.DTOs;

public sealed class UserReservationResponseDto
{
    public Guid ReservationId { get; init; }
    public int UserId { get; init; }
    public int EventId { get; init; }
    public Guid SeatId { get; init; }
    public int SectorId { get; init; }
    public string SectorName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string SeatStatus { get; init; } = string.Empty;
    public string SeatRowIdentifier { get; init; } = string.Empty;
    public int SeatNumber { get; init; }
    public string ReservationStatus { get; init; } = string.Empty;
    public DateTime ReservedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
}
