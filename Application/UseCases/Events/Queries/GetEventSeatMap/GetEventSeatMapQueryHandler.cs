using Application.DTOs;
using Application.Interfaces;
using Domain.Exceptions;
using System.Net;

namespace Application.UseCases.Events.Queries.GetEventSeatMap;

public sealed class GetEventSeatMapQueryHandler : IGetEventSeatMapQueryHandler
{
    private readonly IEventRepository _eventRepository;

    public GetEventSeatMapQueryHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<EventSeatMapResponseDto> Handle(GetEventSeatMapQuery query, CancellationToken cancellationToken = default)
    {
        var eventEntity = await _eventRepository.GetSeatMapByEventIdAsync(query.EventId, cancellationToken);

        if (eventEntity is null)
        {
            throw new DomainException("Event not found.", HttpStatusCode.NotFound);
        }

        return new EventSeatMapResponseDto
        {
            EventId = eventEntity.Id,
            EventName = eventEntity.Name,
            EventDate = eventEntity.EventDate,
            Venue = eventEntity.Venue,
            Sectors = eventEntity.Sectors
                .OrderBy(sector => sector.Id)
                .Select(MapSector)
                .ToArray()
        };
    }

    private static SectorSeatMapResponseDto MapSector(Domain.Entities.Sector sector)
    {
        return new SectorSeatMapResponseDto
        {
            Id = sector.Id,
            Name = sector.Name,
            Price = sector.Price,
            Capacity = sector.Capacity,
            Seats = sector.Seats
                .OrderBy(seat => seat.RowIdentifier)
                .ThenBy(seat => seat.SeatNumber)
                .Select(MapSeat)
                .ToArray()
        };
    }

    private static SeatResponseDto MapSeat(Domain.Entities.Seat seat)
    {
        return new SeatResponseDto
        {
            Id = seat.Id,
            RowIdentifier = seat.RowIdentifier,
            SeatNumber = seat.SeatNumber,
            Status = seat.Status
        };
    }
}
