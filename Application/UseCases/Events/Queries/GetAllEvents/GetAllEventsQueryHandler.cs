using Application.DTOs;
using Application.Common;
using Application.Interfaces;
using Domain.Exceptions;

namespace Application.UseCases.Events.Queries.GetAllEvents;

public sealed class GetAllEventsQueryHandler : IGetAllEventsQueryHandler
{
    private const int MaxPageSize = 100;
    private readonly IEventRepository _eventRepository;

    public GetAllEventsQueryHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<PagedResponseDto<EventResponseDto>> Handle(GetAllEventsQuery query, CancellationToken cancellationToken = default)
    {
        if (query.Page < 1)
        {
            throw new ValidationException("El numero de pagina debe ser mayor a cero.");
        }

        if (query.PageSize < 1 || query.PageSize > MaxPageSize)
        {
            throw new ValidationException($"El tamano de pagina debe estar entre 1 y {MaxPageSize}.");
        }

        var (events, totalCount) = await _eventRepository.GetActiveEventsPagedAsync(query.Page, query.PageSize, cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return new PagedResponseDto<EventResponseDto>
        {
            Items = events
                .Select(MapEvent)
                .ToArray(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalCount,
            TotalPages = totalPages
        };
    }

    private static EventResponseDto MapEvent(Domain.Entities.Event eventEntity)
    {
        return new EventResponseDto
        {
            Id = eventEntity.Id,
            Name = eventEntity.Name,
            EventDate = UtcDateTime.Normalize(eventEntity.EventDate),
            Venue = eventEntity.Venue,
            Status = eventEntity.Status
        };
    }
}
