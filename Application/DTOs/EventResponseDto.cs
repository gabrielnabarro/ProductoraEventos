namespace Application.DTOs;

public sealed class EventResponseDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime EventDate { get; init; }
    public string Venue { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
