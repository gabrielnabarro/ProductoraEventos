namespace Application.DTOs;

public sealed class ReservationResponseDto
{
    public Guid ReservationId { get; init; }
    public Guid SeatId { get; init; }
    public int UserId { get; init; }
    public string SeatStatus { get; init; } = string.Empty;
    public DateTime ReservedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public string Message { get; init; } = string.Empty;
}
