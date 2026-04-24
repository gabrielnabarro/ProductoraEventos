using Application.DTOs;
using Application.UseCases.Events.Queries.GetEventSeatMap;

namespace Application.Interfaces;

public interface IGetEventSeatMapQueryHandler
{
    Task<EventSeatMapResponseDto> Handle(GetEventSeatMapQuery query, CancellationToken cancellationToken = default);
}
