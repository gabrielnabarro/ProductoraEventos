using Application.DTOs;
using Application.UseCases.Events.Queries.GetAllEvents;

namespace Application.Interfaces;

public interface IGetAllEventsQueryHandler
{
    Task<PagedResponseDto<EventResponseDto>> Handle(GetAllEventsQuery query, CancellationToken cancellationToken = default);
}
