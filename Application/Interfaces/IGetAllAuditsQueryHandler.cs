using Application.DTOs;
using Application.UseCases.Audits.Queries.GetAllAudits;

namespace Application.Interfaces;

public interface IGetAllAuditsQueryHandler
{
    Task<PagedResponseDto<AuditLogResponseDto>> Handle(GetAllAuditsQuery query, CancellationToken cancellationToken = default);
}
