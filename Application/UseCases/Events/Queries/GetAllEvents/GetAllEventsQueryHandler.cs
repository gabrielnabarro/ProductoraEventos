using Application.DTOs;
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
            throw new DomainException("The page number must be greater than zero.");
        }

        if (query.PageSize < 1 || query.PageSize > MaxPageSize)
        {
            throw new DomainException($"The page size must be between 1 and {MaxPageSize}.");
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
            EventDate = eventEntity.EventDate,
            Venue = eventEntity.Venue,
            Status = eventEntity.Status
        };
    }
}
