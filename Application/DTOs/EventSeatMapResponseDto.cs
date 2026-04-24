namespace Application.DTOs;

public sealed class EventSeatMapResponseDto
{
    public int EventId { get; init; }
    public string EventName { get; init; } = string.Empty;
    public DateTime EventDate { get; init; }
    public string Venue { get; init; } = string.Empty;
    public IReadOnlyCollection<SectorSeatMapResponseDto> Sectors { get; init; } = Array.Empty<SectorSeatMapResponseDto>();
}

public sealed class SectorSeatMapResponseDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Capacity { get; init; }
    public IReadOnlyCollection<SeatResponseDto> Seats { get; init; } = Array.Empty<SeatResponseDto>();
}

public sealed class SeatResponseDto
{
    public Guid Id { get; init; }
    public string RowIdentifier { get; init; } = string.Empty;
    public int SeatNumber { get; init; }
    public string Status { get; init; } = string.Empty;
}
