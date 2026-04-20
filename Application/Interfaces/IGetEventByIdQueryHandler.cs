using Application.DTOs;
using Application.UseCases.Events.Queries.GetEventById;

namespace Application.Interfaces;

public interface IGetEventByIdQueryHandler
{
    Task<EventResponseDto> Handle(GetEventByIdQuery query, CancellationToken cancellationToken = default);
}
