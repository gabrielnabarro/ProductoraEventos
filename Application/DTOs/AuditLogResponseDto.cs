namespace Application.DTOs;

public sealed class AuditLogResponseDto
{
    public Guid Id { get; init; }
    public int? UserId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;
    public string Details { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
