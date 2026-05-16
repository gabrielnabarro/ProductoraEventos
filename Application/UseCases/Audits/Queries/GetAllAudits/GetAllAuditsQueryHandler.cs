using Application.DTOs;
using Application.Common;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.UseCases.Audits.Queries.GetAllAudits;

public sealed class GetAllAuditsQueryHandler : IGetAllAuditsQueryHandler
{
    private const int MaxPageSize = 100;
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAllAuditsQueryHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<PagedResponseDto<AuditLogResponseDto>> Handle(GetAllAuditsQuery query, CancellationToken cancellationToken = default)
    {
        if (query.Page < 1)
        {
            throw new ValidationException("El numero de pagina debe ser mayor a cero.");
        }

        if (query.PageSize < 1 || query.PageSize > MaxPageSize)
        {
            throw new ValidationException($"El tamano de pagina debe estar entre 1 y {MaxPageSize}.");
        }

        var (auditLogs, totalCount) = await _auditLogRepository.GetPagedAsync(query.Page, query.PageSize, cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return new PagedResponseDto<AuditLogResponseDto>
        {
            Items = auditLogs
                .Select(MapAuditLog)
                .ToArray(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalCount,
            TotalPages = totalPages
        };
    }

    private static AuditLogResponseDto MapAuditLog(AuditLog auditLog)
    {
        return new AuditLogResponseDto
        {
            Id = auditLog.Id,
            UserId = auditLog.UserId,
            Action = auditLog.Action,
            EntityType = auditLog.EntityType,
            EntityId = auditLog.EntityId,
            Details = auditLog.Details,
            CreatedAt = UtcDateTime.Normalize(auditLog.CreatedAt)
        };
    }
}
