using Application.DTOs;
using Application.Interfaces;
using Domain.Exceptions;

namespace Application.UseCases.Events.Queries.GetEventById;

public sealed class GetEventByIdQueryHandler : IGetEventByIdQueryHandler
{
    private readonly IEventRepository _eventRepository;

    public GetEventByIdQueryHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<EventResponseDto> Handle(GetEventByIdQuery query, CancellationToken cancellationToken = default)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(query.EventId, cancellationToken);

        if (eventEntity is null)
        {
            throw new NotFoundException("Evento no encontrado.");
        }

        return new EventResponseDto
        {
            Id = eventEntity.Id,
            Name = eventEntity.Name,
            EventDate = eventEntity.EventDate,
            Venue = eventEntity.Venue,
            Status = eventEntity.Status
        };
    }
}
