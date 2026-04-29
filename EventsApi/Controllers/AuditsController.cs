using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.Audits.Queries.GetAllAudits;
using Microsoft.AspNetCore.Mvc;

namespace EventsApi.Controllers;

[ApiController]
[Route("api/v1/audits")]
public sealed class AuditsController : ControllerBase
{
    private readonly IGetAllAuditsQueryHandler _getAllAuditsQueryHandler;

    public AuditsController(IGetAllAuditsQueryHandler getAllAuditsQueryHandler)
    {
        _getAllAuditsQueryHandler = getAllAuditsQueryHandler;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDto<AuditLogResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponseDto<AuditLogResponseDto>>> GetAudits([FromQuery] GetAllAuditsQuery query, CancellationToken cancellationToken)
    {
        var response = await _getAllAuditsQueryHandler.Handle(query, cancellationToken);
        return Ok(response);
    }
}
